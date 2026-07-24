using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.Analysis.Mapping;
using CodeSage.Application.Features.Analysis.Models;
using CodeSage.Application.Features.GitHub.Queries;
using CodeSage.Application.Interfaces;
using MediatR;

namespace CodeSage.Application.Features.Analysis.Queries;

/// <summary>
/// Fetches a pull request from GitHub and produces a provider-agnostic <see cref="ReviewContext"/>.
/// </summary>
public sealed record AnalyzePullRequestQuery(string Owner, string Name, int Number)
    : IRequest<ReviewContext>;

public sealed class AnalyzePullRequestQueryHandler(
    IGitHubClient gitHubClient,
    IGitHubAccessTokenAccessor tokenAccessor,
    IPullRequestAnalysisEngine analysisEngine)
    : IRequestHandler<AnalyzePullRequestQuery, ReviewContext>
{
    public async Task<ReviewContext> Handle(
        AnalyzePullRequestQuery request,
        CancellationToken cancellationToken)
    {
        var token = await GitHubTokenGuard.RequireTokenAsync(tokenAccessor, cancellationToken)
            .ConfigureAwait(false);

        // Fetch repository metadata + PR details; analysis never sees GitHub SDK types.
        var repositoryTask = gitHubClient.GetRepositoryAsync(
            token,
            request.Owner,
            request.Name,
            cancellationToken);
        var pullRequestTask = gitHubClient.GetPullRequestAsync(
            token,
            request.Owner,
            request.Name,
            request.Number,
            cancellationToken);

        await Task.WhenAll(repositoryTask, pullRequestTask).ConfigureAwait(false);

        var repository = await repositoryTask.ConfigureAwait(false);
        var pullRequest = await pullRequestTask.ConfigureAwait(false);

        var input = PullRequestAnalysisInputMapper.FromGitHub(
            repository.Name,
            repository.FullName,
            repository.DefaultBranch,
            pullRequest);

        return analysisEngine.Analyze(input);
    }
}
