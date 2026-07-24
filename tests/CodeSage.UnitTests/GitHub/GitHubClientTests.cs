using System.Net;
using System.Net.Http.Headers;
using CodeSage.Application.Common.Exceptions;
using CodeSage.Infrastructure.GitHub;
using CodeSage.Infrastructure.Options;
using CodeSage.UnitTests.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CodeSage.UnitTests.GitHub;

public sealed class GitHubClientTests
{
    private const string AccessToken = "gho_test_token";

    [Fact]
    public async Task ListRepositoriesAsync_MapsGitHubPayload_ToApplicationDtos()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/user/repos", HttpStatusCode.OK, ReadFixture("repositories.json"));
        var sut = CreateClient(handler);

        var repositories = await sut.ListRepositoriesAsync(AccessToken);

        repositories.Should().ContainSingle();
        var repository = repositories[0];
        repository.FullName.Should().Be("octocat/codesage");
        repository.OwnerLogin.Should().Be("octocat");
        repository.DefaultBranch.Should().Be("main");
        handler.Requests.Should().ContainSingle(request =>
            request.Headers.Authorization!.Scheme == "Bearer"
            && request.Headers.Authorization.Parameter == AccessToken);
    }

    [Fact]
    public async Task GetRepositoryAsync_ReturnsDetails()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/repos/octocat/codesage", HttpStatusCode.OK, ReadFixture("repository.json"));
        var sut = CreateClient(handler);

        var repository = await sut.GetRepositoryAsync(AccessToken, "octocat", "codesage");

        repository.Language.Should().Be("C#");
        repository.StargazersCount.Should().Be(10);
        repository.OpenIssuesCount.Should().Be(3);
    }

    [Fact]
    public async Task ListPullRequestsAsync_ReturnsSummaries()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/repos/octocat/codesage/pulls?", HttpStatusCode.OK, ReadFixture("pull-requests.json"));
        var sut = CreateClient(handler);

        var pullRequests = await sut.ListPullRequestsAsync(AccessToken, "octocat", "codesage");

        pullRequests.Should().ContainSingle();
        pullRequests[0].Number.Should().Be(42);
        pullRequests[0].Title.Should().Be("Add GitHub integration");
        pullRequests[0].AuthorLogin.Should().Be("octocat");
    }

    [Fact]
    public async Task GetPullRequestAsync_AggregatesFilesCommitsAndComments()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/pulls/42/files", HttpStatusCode.OK, ReadFixture("pull-request-files.json"))
            .MapGet("/pulls/42/commits", HttpStatusCode.OK, ReadFixture("pull-request-commits.json"))
            .MapGet("/pulls/42/comments", HttpStatusCode.OK, ReadFixture("review-comments.json"))
            .MapGet("/issues/42/comments", HttpStatusCode.OK, ReadFixture("issue-comments.json"))
            .MapGet("/pulls/42", HttpStatusCode.OK, ReadFixture("pull-request.json"));

        var sut = CreateClient(handler);

        var pullRequest = await sut.GetPullRequestAsync(AccessToken, "octocat", "codesage", 42);

        pullRequest.Title.Should().Be("Add GitHub integration");
        pullRequest.Description.Should().Contain("OAuth");
        pullRequest.AuthorLogin.Should().Be("octocat");
        pullRequest.BaseRef.Should().Be("main");
        pullRequest.HeadRef.Should().Be("feature/github");
        pullRequest.ChangedFiles.Should().ContainSingle(file => file.Filename.EndsWith("Program.cs"));
        pullRequest.Commits.Should().ContainSingle(commit => commit.Sha.StartsWith("abc123"));
        pullRequest.Comments.Should().HaveCount(2);
        pullRequest.Comments.Should().Contain(comment => comment.Kind == "issue");
        pullRequest.Comments.Should().Contain(comment =>
            comment.Kind == "review" && comment.Path!.Contains("GitHubClient.cs"));
    }

    [Fact]
    public async Task GetAuthenticatedUserAsync_MapsUser()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/user", HttpStatusCode.OK, ReadFixture("user.json"));
        var sut = CreateClient(handler);

        var user = await sut.GetAuthenticatedUserAsync(AccessToken);

        user.Login.Should().Be("octocat");
        user.Name.Should().Be("The Octocat");
    }

    [Fact]
    public async Task GetRepositoryAsync_WhenNotFound_ThrowsGitHubNotFoundException()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/repos/octocat/missing", HttpStatusCode.NotFound, """{"message":"Not Found"}""");
        var sut = CreateClient(handler);

        var act = async () => await sut.GetRepositoryAsync(AccessToken, "octocat", "missing");

        await act.Should().ThrowAsync<GitHubNotFoundException>()
            .WithMessage("*Not Found*");
    }

    [Fact]
    public async Task ListRepositoriesAsync_WhenUnauthorized_ThrowsGitHubUnauthorizedException()
    {
        var handler = new StubHttpMessageHandler()
            .MapGet("/user/repos", HttpStatusCode.Unauthorized, """{"message":"Bad credentials"}""");
        var sut = CreateClient(handler);

        var act = async () => await sut.ListRepositoriesAsync(AccessToken);

        await act.Should().ThrowAsync<GitHubUnauthorizedException>();
    }

    [Fact]
    public async Task ListRepositoriesAsync_WhenRateLimited_ThrowsGitHubRateLimitExceededException()
    {
        var sut = CreateClient(new RateLimitHandler());

        var act = async () => await sut.ListRepositoriesAsync(AccessToken);

        await act.Should().ThrowAsync<GitHubRateLimitExceededException>();
    }

    private static GitHubClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CodeSage", "0.1.0"));

        var options = Options.Create(new GitHubOptions
        {
            ApiBaseUrl = "https://api.github.com/",
            UserAgent = "CodeSage"
        });

        return new GitHubClient(httpClient, options, NullLogger<GitHubClient>.Instance);
    }

    private static string ReadFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "GitHub", fileName);
        return File.ReadAllText(path);
    }

    private sealed class RateLimitHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("""{"message":"API rate limit exceeded"}""")
            };
            response.Headers.Add("X-RateLimit-Remaining", "0");
            return Task.FromResult(response);
        }
    }
}
