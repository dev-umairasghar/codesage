using CodeSage.Application.Features.GitHub.Dtos;

namespace CodeSage.Application.Interfaces;

/// <summary>
/// Outbound port for GitHub REST operations.
/// Implementations live in Infrastructure and must never leak vendor SDK types.
/// </summary>
public interface IGitHubClient
{
    Task<IReadOnlyList<RepositorySummaryDto>> ListRepositoriesAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<RepositoryDetailsDto> GetRepositoryAsync(
        string accessToken,
        string owner,
        string name,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PullRequestSummaryDto>> ListPullRequestsAsync(
        string accessToken,
        string owner,
        string name,
        CancellationToken cancellationToken = default);

    Task<PullRequestDetailsDto> GetPullRequestAsync(
        string accessToken,
        string owner,
        string name,
        int number,
        CancellationToken cancellationToken = default);

    Task<GitHubUserDto> GetAuthenticatedUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
