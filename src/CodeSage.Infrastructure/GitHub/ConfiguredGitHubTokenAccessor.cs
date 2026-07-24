using CodeSage.Application.Interfaces;
using CodeSage.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CodeSage.Infrastructure.GitHub;

/// <summary>
/// Supplies the configured Personal Access Token to Application handlers.
/// No OAuth / user accounts — local-first PAT from configuration.
/// </summary>
public sealed class ConfiguredGitHubTokenAccessor(IOptions<GitHubOptions> options)
    : IGitHubAccessTokenAccessor
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var token = options.Value.PersonalAccessToken;
        return Task.FromResult<string?>(string.IsNullOrWhiteSpace(token) ? null : token);
    }
}
