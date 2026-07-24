namespace CodeSage.Application.Features.Analysis.Models;

/// <summary>
/// Programming languages / formats CodeSage can classify in Stage 3.
/// Unknown and Binary are explicit so future AI providers can filter safely.
/// </summary>
public enum CodeLanguage
{
    Unknown = 0,
    CSharp,
    Sql,
    JavaScript,
    TypeScript,
    Json,
    Yaml,
    Markdown,
    Xml,
    Binary
}

/// <summary>
/// Normalized change status — independent of any source-control vendor vocabulary.
/// </summary>
public enum FileChangeStatus
{
    Unknown = 0,
    New,
    Modified,
    Deleted,
    Renamed
}

/// <summary>
/// Provider-agnostic input to the analysis engine.
/// Built from GitHub (or later Azure DevOps) at the boundary — never used by AI providers.
/// </summary>
public sealed record PullRequestAnalysisInput(
    string RepositoryName,
    string RepositoryFullName,
    string? RepositoryDefaultBranch,
    int PullRequestNumber,
    string Title,
    string? Description,
    string State,
    bool IsDraft,
    string AuthorLogin,
    string? AuthorDisplayName,
    string? AuthorAvatarUrl,
    string BaseRef,
    string HeadRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<CommitAnalysisInput> Commits,
    IReadOnlyList<ChangedFileAnalysisInput> ChangedFiles);

public sealed record CommitAnalysisInput(
    string Sha,
    string Message,
    string AuthorName,
    string? AuthorLogin,
    DateTimeOffset? CommittedAt);

public sealed record ChangedFileAnalysisInput(
    string Filename,
    string RawStatus,
    int Additions,
    int Deletions,
    int Changes,
    string? Patch);

/// <summary>
/// The only object future AI providers should consume.
/// Contains no GitHub / Octokit / vendor types — only CodeSage vocabulary.
/// </summary>
public sealed record ReviewContext(
    ReviewRepositoryInfo Repository,
    ReviewPullRequestInfo PullRequest,
    ReviewAuthorInfo Author,
    IReadOnlyList<ReviewCommitInfo> Commits,
    IReadOnlyList<ReviewFileChange> ChangedFiles,
    ReviewStatistics Statistics,
    IReadOnlyDictionary<string, int> LanguageBreakdown,
    string Summary);

public sealed record ReviewRepositoryInfo(
    string Name,
    string FullName,
    string? DefaultBranch);

public sealed record ReviewPullRequestInfo(
    int Number,
    string Title,
    string? Description,
    string State,
    bool IsDraft,
    string BaseRef,
    string HeadRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReviewAuthorInfo(
    string Login,
    string? DisplayName,
    string? AvatarUrl);

public sealed record ReviewCommitInfo(
    string Sha,
    string Message,
    string AuthorName,
    string? AuthorLogin,
    DateTimeOffset? CommittedAt);

public sealed record ReviewFileChange(
    string Filename,
    string Extension,
    CodeLanguage Language,
    FileChangeStatus Status,
    int Additions,
    int Deletions,
    int TotalChanges,
    string? Patch,
    bool IsBinary,
    bool IsSupported);

public sealed record ReviewStatistics(
    int FileCount,
    int CommitCount,
    int Additions,
    int Deletions,
    int TotalChangedLines,
    IReadOnlyList<string> LanguagesUsed,
    string? LargestModifiedFile,
    int LargestModifiedFileChanges,
    int TestFilesChanged,
    int SqlFilesChanged,
    int ConfigurationFilesChanged,
    int ControllerFilesChanged,
    int ServiceFilesChanged,
    bool SqlModified);
