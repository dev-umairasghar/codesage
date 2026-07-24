namespace CodeSage.Infrastructure.Options;

/// <summary>
/// Local-first GitHub configuration. Bound from the <c>GitHub</c> section.
/// Use a Personal Access Token (classic or fine-grained) — never commit secrets.
/// </summary>
public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";

    /// <summary>
    /// Personal Access Token used for GitHub REST calls.
    /// Prefer user-secrets or environment variable <c>GitHub__PersonalAccessToken</c>.
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// GitHub REST API base address.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.github.com/";

    /// <summary>
    /// Required by GitHub — identify the product in User-Agent.
    /// </summary>
    public string UserAgent { get; set; } = "CodeSage";

    public bool HasPersonalAccessToken => !string.IsNullOrWhiteSpace(PersonalAccessToken);
}
