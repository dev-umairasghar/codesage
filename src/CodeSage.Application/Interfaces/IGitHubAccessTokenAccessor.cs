namespace CodeSage.Application.Interfaces;

/// <summary>
/// Resolves the configured GitHub Personal Access Token for Application handlers.
/// Local-first — no OAuth or user sessions.
/// </summary>
public interface IGitHubAccessTokenAccessor
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
