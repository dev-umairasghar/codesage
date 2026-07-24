using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.GitHub.Dtos;
using CodeSage.Application.Interfaces;
using MediatR;

namespace CodeSage.Application.Features.GitHub.Queries;

public sealed record GetAuthenticatedGitHubUserQuery : IRequest<GitHubUserDto>;

public sealed record ListRepositoriesQuery : IRequest<IReadOnlyList<RepositorySummaryDto>>;

public sealed record GetRepositoryQuery(string Owner, string Name) : IRequest<RepositoryDetailsDto>;

public sealed record ListPullRequestsQuery(string Owner, string Name)
    : IRequest<IReadOnlyList<PullRequestSummaryDto>>;

public sealed record GetPullRequestQuery(string Owner, string Name, int Number)
    : IRequest<PullRequestDetailsDto>;

/// <summary>
/// Shared helper: resolve the configured Personal Access Token or fail clearly.
/// </summary>
internal static class GitHubTokenGuard
{
    public static async Task<string> RequireTokenAsync(
        IGitHubAccessTokenAccessor tokenAccessor,
        CancellationToken cancellationToken)
    {
        var token = await tokenAccessor.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new GitHubUnauthorizedException(
                "Missing GitHub Personal Access Token. Set GitHub:PersonalAccessToken via user-secrets "
                + "or environment variable GitHub__PersonalAccessToken. See docs/Configuration.md.");
        }

        return token;
    }
}

public sealed class GetAuthenticatedGitHubUserQueryHandler(
    IGitHubClient gitHubClient,
    IGitHubAccessTokenAccessor tokenAccessor)
    : IRequestHandler<GetAuthenticatedGitHubUserQuery, GitHubUserDto>
{
    public async Task<GitHubUserDto> Handle(
        GetAuthenticatedGitHubUserQuery request,
        CancellationToken cancellationToken)
    {
        var token = await GitHubTokenGuard.RequireTokenAsync(tokenAccessor, cancellationToken)
            .ConfigureAwait(false);
        return await gitHubClient.GetAuthenticatedUserAsync(token, cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class ListRepositoriesQueryHandler(
    IGitHubClient gitHubClient,
    IGitHubAccessTokenAccessor tokenAccessor)
    : IRequestHandler<ListRepositoriesQuery, IReadOnlyList<RepositorySummaryDto>>
{
    public async Task<IReadOnlyList<RepositorySummaryDto>> Handle(
        ListRepositoriesQuery request,
        CancellationToken cancellationToken)
    {
        var token = await GitHubTokenGuard.RequireTokenAsync(tokenAccessor, cancellationToken)
            .ConfigureAwait(false);
        return await gitHubClient.ListRepositoriesAsync(token, cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetRepositoryQueryHandler(
    IGitHubClient gitHubClient,
    IGitHubAccessTokenAccessor tokenAccessor)
    : IRequestHandler<GetRepositoryQuery, RepositoryDetailsDto>
{
    public async Task<RepositoryDetailsDto> Handle(
        GetRepositoryQuery request,
        CancellationToken cancellationToken)
    {
        var token = await GitHubTokenGuard.RequireTokenAsync(tokenAccessor, cancellationToken)
            .ConfigureAwait(false);
        return await gitHubClient
            .GetRepositoryAsync(token, request.Owner, request.Name, cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class ListPullRequestsQueryHandler(
    IGitHubClient gitHubClient,
    IGitHubAccessTokenAccessor tokenAccessor)
    : IRequestHandler<ListPullRequestsQuery, IReadOnlyList<PullRequestSummaryDto>>
{
    public async Task<IReadOnlyList<PullRequestSummaryDto>> Handle(
        ListPullRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var token = await GitHubTokenGuard.RequireTokenAsync(tokenAccessor, cancellationToken)
            .ConfigureAwait(false);
        return await gitHubClient
            .ListPullRequestsAsync(token, request.Owner, request.Name, cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetPullRequestQueryHandler(
    IGitHubClient gitHubClient,
    IGitHubAccessTokenAccessor tokenAccessor)
    : IRequestHandler<GetPullRequestQuery, PullRequestDetailsDto>
{
    public async Task<PullRequestDetailsDto> Handle(
        GetPullRequestQuery request,
        CancellationToken cancellationToken)
    {
        var token = await GitHubTokenGuard.RequireTokenAsync(tokenAccessor, cancellationToken)
            .ConfigureAwait(false);
        return await gitHubClient
            .GetPullRequestAsync(token, request.Owner, request.Name, request.Number, cancellationToken)
            .ConfigureAwait(false);
    }
}
