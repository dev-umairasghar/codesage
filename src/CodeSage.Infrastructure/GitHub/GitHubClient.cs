using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.GitHub.Dtos;
using CodeSage.Application.Interfaces;
using CodeSage.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSage.Infrastructure.GitHub;

/// <summary>
/// HttpClient-based GitHub REST adapter.
/// Prefer raw HTTP over a vendor SDK here so:
/// 1) Application DTOs stay free of SDK types, and
/// 2) unit tests can inject mocked HTTP responses without Octokit fakes.
/// </summary>
public sealed class GitHubClient(
    HttpClient httpClient,
    IOptions<GitHubOptions> options,
    ILogger<GitHubClient> logger) : IGitHubClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly GitHubOptions _options = options.Value;

    public Task<GitHubUserDto> GetAuthenticatedUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default) =>
        GetAsync<GitHubUserResponse, GitHubUserDto>(
            accessToken,
            "user",
            GitHubResponseMapper.ToUser,
            cancellationToken);

    public async Task<IReadOnlyList<RepositorySummaryDto>> ListRepositoriesAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        // affiliation covers owned + collaborator + organization memberships the token can see.
        var path = "user/repos?per_page=100&sort=updated&affiliation=owner,collaborator,organization_member";
        var repos = await GetAsync<List<GitHubRepositoryResponse>>(accessToken, path, cancellationToken)
            .ConfigureAwait(false);

        return repos.Select(GitHubResponseMapper.ToRepositorySummary).ToList();
    }

    public Task<RepositoryDetailsDto> GetRepositoryAsync(
        string accessToken,
        string owner,
        string name,
        CancellationToken cancellationToken = default) =>
        GetAsync<GitHubRepositoryResponse, RepositoryDetailsDto>(
            accessToken,
            $"repos/{Encode(owner)}/{Encode(name)}",
            GitHubResponseMapper.ToRepositoryDetails,
            cancellationToken);

    public async Task<IReadOnlyList<PullRequestSummaryDto>> ListPullRequestsAsync(
        string accessToken,
        string owner,
        string name,
        CancellationToken cancellationToken = default)
    {
        var path = $"repos/{Encode(owner)}/{Encode(name)}/pulls?state=all&per_page=50&sort=updated";
        var pullRequests = await GetAsync<List<GitHubPullRequestResponse>>(accessToken, path, cancellationToken)
            .ConfigureAwait(false);

        return pullRequests.Select(GitHubResponseMapper.ToPullRequestSummary).ToList();
    }

    public async Task<PullRequestDetailsDto> GetPullRequestAsync(
        string accessToken,
        string owner,
        string name,
        int number,
        CancellationToken cancellationToken = default)
    {
        var ownerSegment = Encode(owner);
        var nameSegment = Encode(name);
        var basePath = $"repos/{ownerSegment}/{nameSegment}";

        // Fetch PR metadata and related collections concurrently — separate endpoints on GitHub.
        var pullRequestTask = GetAsync<GitHubPullRequestResponse>(
            accessToken,
            $"{basePath}/pulls/{number}",
            cancellationToken);
        var filesTask = GetAsync<List<GitHubPullRequestFileResponse>>(
            accessToken,
            $"{basePath}/pulls/{number}/files?per_page=100",
            cancellationToken);
        var commitsTask = GetAsync<List<GitHubCommitResponse>>(
            accessToken,
            $"{basePath}/pulls/{number}/commits?per_page=100",
            cancellationToken);
        var issueCommentsTask = GetAsync<List<GitHubIssueCommentResponse>>(
            accessToken,
            $"{basePath}/issues/{number}/comments?per_page=100",
            cancellationToken);
        var reviewCommentsTask = GetAsync<List<GitHubReviewCommentResponse>>(
            accessToken,
            $"{basePath}/pulls/{number}/comments?per_page=100",
            cancellationToken);

        await Task.WhenAll(pullRequestTask, filesTask, commitsTask, issueCommentsTask, reviewCommentsTask)
            .ConfigureAwait(false);

        var pullRequest = await pullRequestTask.ConfigureAwait(false);
        var files = await filesTask.ConfigureAwait(false);
        var commits = await commitsTask.ConfigureAwait(false);
        var issueComments = await issueCommentsTask.ConfigureAwait(false);
        var reviewComments = await reviewCommentsTask.ConfigureAwait(false);

        var comments = issueComments
            .Select(GitHubResponseMapper.ToIssueComment)
            .Concat(reviewComments.Select(GitHubResponseMapper.ToReviewComment))
            .OrderBy(comment => comment.CreatedAt)
            .ToList();

        return GitHubResponseMapper.ToPullRequestDetails(
            pullRequest,
            files.Select(GitHubResponseMapper.ToChangedFile).ToList(),
            commits.Select(GitHubResponseMapper.ToCommit).ToList(),
            comments);
    }

    private async Task<TDto> GetAsync<TResponse, TDto>(
        string accessToken,
        string relativePath,
        Func<TResponse, TDto> map,
        CancellationToken cancellationToken)
    {
        var response = await GetAsync<TResponse>(accessToken, relativePath, cancellationToken)
            .ConfigureAwait(false);
        return map(response);
    }

    private async Task<T> GetAsync<T>(
        string accessToken,
        string relativePath,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");

        // User-Agent is required by GitHub; set on HttpClient in DI, reinforce here for safety.
        if (!request.Headers.UserAgent.Any())
        {
            request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        }

        HttpResponseMessage response;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("Calling GitHub API {Method} {Path}", HttpMethod.Get.Method, relativePath);
            response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "GitHub request timed out for {Path}", relativePath);
            throw new GitHubApiException("GitHub API request timed out.", innerException: exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "GitHub request failed for {Path}", relativePath);
            throw new GitHubApiException("Unable to reach GitHub API.", innerException: exception);
        }

        using (response)
        {
            stopwatch.Stop();
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "GitHub API {Path} succeeded in {ElapsedMs} ms with {StatusCode}",
                    relativePath,
                    stopwatch.ElapsedMilliseconds,
                    (int)response.StatusCode);

                var payload = await response.Content
                    .ReadFromJsonAsync<T>(SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (payload is null)
                {
                    throw new GitHubApiException("GitHub API returned an empty response body.");
                }

                return payload;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            logger.LogWarning(
                "GitHub API returned {StatusCode} for {Path} in {ElapsedMs} ms. Body: {Body}",
                (int)response.StatusCode,
                relativePath,
                stopwatch.ElapsedMilliseconds,
                Truncate(body, 500));

            throw MapFailure(response.StatusCode, body, response.Headers);
        }
    }

    private static Exception MapFailure(
        HttpStatusCode statusCode,
        string body,
        HttpResponseHeaders headers)
    {
        var message = ExtractErrorMessage(body) ?? $"GitHub API request failed with status {(int)statusCode}.";

        if (statusCode == HttpStatusCode.NotFound)
        {
            return new GitHubNotFoundException(message);
        }

        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            if (headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues)
                && remainingValues.FirstOrDefault() == "0")
            {
                return new GitHubRateLimitExceededException("GitHub API rate limit exceeded. Try again later.");
            }

            return new GitHubUnauthorizedException(message);
        }

        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return new GitHubRateLimitExceededException(message);
        }

        return new GitHubApiException(message, (int)statusCode);
    }

    private static string? ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
        }
        catch (JsonException)
        {
            // Fall through — return truncated raw body.
        }

        return Truncate(body, 200);
    }

    private static string Encode(string value) => Uri.EscapeDataString(value);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
