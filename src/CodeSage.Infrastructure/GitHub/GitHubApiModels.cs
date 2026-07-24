using System.Text.Json.Serialization;

namespace CodeSage.Infrastructure.GitHub;

/// <summary>
/// Internal JSON shapes matching GitHub REST payloads.
/// Kept internal so Octokit/SDK/GitHub wire formats never leave Infrastructure.
/// </summary>
internal sealed class GitHubUserResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
}

internal sealed class GitHubRepositoryResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("open_issues_count")]
    public int OpenIssuesCount { get; set; }

    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("owner")]
    public GitHubUserResponse? Owner { get; set; }
}

internal sealed class GitHubPullRequestResponse
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("user")]
    public GitHubUserResponse? User { get; set; }

    [JsonPropertyName("base")]
    public GitHubPullRequestRefResponse? Base { get; set; }

    [JsonPropertyName("head")]
    public GitHubPullRequestRefResponse? Head { get; set; }
}

internal sealed class GitHubPullRequestRefResponse
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; } = string.Empty;
}

internal sealed class GitHubPullRequestFileResponse
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    [JsonPropertyName("changes")]
    public int Changes { get; set; }

    [JsonPropertyName("patch")]
    public string? Patch { get; set; }
}

internal sealed class GitHubCommitResponse
{
    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;

    [JsonPropertyName("commit")]
    public GitHubCommitDetailResponse? Commit { get; set; }

    [JsonPropertyName("author")]
    public GitHubUserResponse? Author { get; set; }
}

internal sealed class GitHubCommitDetailResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public GitHubCommitAuthorResponse? Author { get; set; }
}

internal sealed class GitHubCommitAuthorResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTimeOffset? Date { get; set; }
}

internal sealed class GitHubIssueCommentResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("user")]
    public GitHubUserResponse? User { get; set; }
}

internal sealed class GitHubReviewCommentResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("line")]
    public int? Line { get; set; }

    [JsonPropertyName("user")]
    public GitHubUserResponse? User { get; set; }
}
