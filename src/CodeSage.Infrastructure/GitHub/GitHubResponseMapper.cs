using CodeSage.Application.Features.GitHub.Dtos;

namespace CodeSage.Infrastructure.GitHub;

/// <summary>
/// Maps internal GitHub wire models to Application DTOs.
/// Isolated so mapping rules are unit-testable without HTTP.
/// </summary>
internal static class GitHubResponseMapper
{
    public static GitHubUserDto ToUser(GitHubUserResponse response) =>
        new(
            response.Id,
            response.Login,
            response.Name,
            response.AvatarUrl,
            response.HtmlUrl);

    public static RepositorySummaryDto ToRepositorySummary(GitHubRepositoryResponse response) =>
        new(
            response.Id,
            response.Name,
            response.FullName,
            response.Owner?.Login ?? string.Empty,
            response.Description,
            response.Private,
            response.HtmlUrl,
            response.DefaultBranch,
            response.UpdatedAt);

    public static RepositoryDetailsDto ToRepositoryDetails(GitHubRepositoryResponse response) =>
        new(
            response.Id,
            response.Name,
            response.FullName,
            response.Owner?.Login ?? string.Empty,
            response.Description,
            response.Private,
            response.HtmlUrl,
            response.DefaultBranch,
            response.Language,
            response.OpenIssuesCount,
            response.ForksCount,
            response.StargazersCount,
            response.CreatedAt,
            response.UpdatedAt);

    public static PullRequestSummaryDto ToPullRequestSummary(GitHubPullRequestResponse response) =>
        new(
            response.Number,
            response.Title,
            response.State,
            response.Draft,
            response.User?.Login ?? string.Empty,
            response.User?.AvatarUrl,
            response.CreatedAt,
            response.UpdatedAt,
            response.HtmlUrl);

    public static ChangedFileDto ToChangedFile(GitHubPullRequestFileResponse response) =>
        new(
            response.Filename,
            response.Status,
            response.Additions,
            response.Deletions,
            response.Changes,
            response.Patch);

    public static CommitSummaryDto ToCommit(GitHubCommitResponse response) =>
        new(
            response.Sha,
            response.Commit?.Message ?? string.Empty,
            response.Commit?.Author?.Name ?? response.Author?.Login ?? string.Empty,
            response.Author?.Login,
            response.Commit?.Author?.Date);

    public static PullRequestCommentDto ToIssueComment(GitHubIssueCommentResponse response) =>
        new(
            response.Id,
            response.User?.Login ?? string.Empty,
            response.Body,
            response.CreatedAt,
            Kind: "issue",
            Path: null,
            Line: null);

    public static PullRequestCommentDto ToReviewComment(GitHubReviewCommentResponse response) =>
        new(
            response.Id,
            response.User?.Login ?? string.Empty,
            response.Body,
            response.CreatedAt,
            Kind: "review",
            response.Path,
            response.Line);

    public static PullRequestDetailsDto ToPullRequestDetails(
        GitHubPullRequestResponse pullRequest,
        IReadOnlyList<ChangedFileDto> files,
        IReadOnlyList<CommitSummaryDto> commits,
        IReadOnlyList<PullRequestCommentDto> comments) =>
        new(
            pullRequest.Number,
            pullRequest.Title,
            pullRequest.Body,
            pullRequest.State,
            pullRequest.Draft,
            pullRequest.User?.Login ?? string.Empty,
            pullRequest.User?.AvatarUrl,
            pullRequest.CreatedAt,
            pullRequest.UpdatedAt,
            pullRequest.HtmlUrl,
            pullRequest.Base?.Ref ?? string.Empty,
            pullRequest.Head?.Ref ?? string.Empty,
            files,
            commits,
            comments);
}
