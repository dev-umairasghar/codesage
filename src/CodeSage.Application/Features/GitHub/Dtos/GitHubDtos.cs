namespace CodeSage.Application.Features.GitHub.Dtos;

/// <summary>
/// Authenticated GitHub user profile (application DTO — not an Octokit/SDK type).
/// </summary>
public sealed record GitHubUserDto(
    long Id,
    string Login,
    string? Name,
    string? AvatarUrl,
    string HtmlUrl);

/// <summary>
/// Compact repository listing item.
/// </summary>
public sealed record RepositorySummaryDto(
    long Id,
    string Name,
    string FullName,
    string OwnerLogin,
    string? Description,
    bool Private,
    string HtmlUrl,
    string DefaultBranch,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Detailed repository view.
/// </summary>
public sealed record RepositoryDetailsDto(
    long Id,
    string Name,
    string FullName,
    string OwnerLogin,
    string? Description,
    bool Private,
    string HtmlUrl,
    string DefaultBranch,
    string? Language,
    int OpenIssuesCount,
    int ForksCount,
    int StargazersCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Compact pull request listing item.
/// </summary>
public sealed record PullRequestSummaryDto(
    int Number,
    string Title,
    string State,
    bool Draft,
    string AuthorLogin,
    string? AuthorAvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string HtmlUrl);

/// <summary>
/// Full pull request details including files, commits, and comments.
/// </summary>
public sealed record PullRequestDetailsDto(
    int Number,
    string Title,
    string? Description,
    string State,
    bool Draft,
    string AuthorLogin,
    string? AuthorAvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string HtmlUrl,
    string BaseRef,
    string HeadRef,
    IReadOnlyList<ChangedFileDto> ChangedFiles,
    IReadOnlyList<CommitSummaryDto> Commits,
    IReadOnlyList<PullRequestCommentDto> Comments);

/// <summary>
/// A file changed in a pull request.
/// </summary>
public sealed record ChangedFileDto(
    string Filename,
    string Status,
    int Additions,
    int Deletions,
    int Changes,
    string? Patch);

/// <summary>
/// Commit summary associated with a pull request.
/// </summary>
public sealed record CommitSummaryDto(
    string Sha,
    string Message,
    string AuthorName,
    string? AuthorLogin,
    DateTimeOffset? CommittedAt);

/// <summary>
/// Issue or review comment on a pull request.
/// </summary>
public sealed record PullRequestCommentDto(
    long Id,
    string AuthorLogin,
    string Body,
    DateTimeOffset CreatedAt,
    string Kind,
    string? Path,
    int? Line);
