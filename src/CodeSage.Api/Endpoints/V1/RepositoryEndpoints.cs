using CodeSage.Application.Features.Analysis.Models;
using CodeSage.Application.Features.Analysis.Queries;
using CodeSage.Application.Features.GitHub.Dtos;
using CodeSage.Application.Features.GitHub.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CodeSage.Api.Endpoints.V1;

/// <summary>
/// Repository and pull-request browsing endpoints (API v1).
/// </summary>
public static class RepositoryEndpoints
{
    public static RouteGroupBuilder MapRepositoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/repositories", ListRepositories)
            .WithName("ListRepositoriesV1")
            .WithTags("Repositories")
            .WithSummary("List repositories")
            .WithDescription(
                "Lists repositories visible to the configured GitHub Personal Access Token "
                + "(owned, collaborator, and organization memberships the token can see).")
            .Produces<IReadOnlyList<RepositorySummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/repositories/{owner}/{name}", GetRepository)
            .WithName("GetRepositoryV1")
            .WithTags("Repositories")
            .WithSummary("Get repository")
            .WithDescription("Returns details for a single repository identified by owner and name.")
            .Produces<RepositoryDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/repositories/{owner}/{name}/pull-requests", ListPullRequests)
            .WithName("ListPullRequestsV1")
            .WithTags("Pull Requests")
            .WithSummary("List pull requests")
            .WithDescription("Lists pull requests for a repository (all states, recently updated first).")
            .Produces<IReadOnlyList<PullRequestSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/repositories/{owner}/{name}/pull-requests/{number:int}", GetPullRequest)
            .WithName("GetPullRequestV1")
            .WithTags("Pull Requests")
            .WithSummary("Get pull request")
            .WithDescription(
                "Returns pull request details including files, commits, and review/issue comments.")
            .Produces<PullRequestDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        group.MapGet("/repositories/{owner}/{name}/pull-requests/{number:int}/analysis", AnalyzePullRequest)
            .WithName("AnalyzePullRequestV1")
            .WithTags("Pull Requests")
            .WithSummary("Analyze pull request")
            .WithDescription(
                "Builds a deterministic ReviewContext for the pull request. "
                + "Does not call OpenAI. Pass the result to POST /api/v1/reviews for an AI review.")
            .Produces<ReviewContext>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return group;
    }

    private static async Task<Ok<IReadOnlyList<RepositorySummaryDto>>> ListRepositories(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListRepositoriesQuery(), cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<RepositoryDetailsDto>> GetRepository(
        [FromRoute] string owner,
        [FromRoute] string name,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRepositoryQuery(owner, name), cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<PullRequestSummaryDto>>> ListPullRequests(
        [FromRoute] string owner,
        [FromRoute] string name,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPullRequestsQuery(owner, name), cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PullRequestDetailsDto>> GetPullRequest(
        [FromRoute] string owner,
        [FromRoute] string name,
        [FromRoute] int number,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPullRequestQuery(owner, name, number), cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<ReviewContext>> AnalyzePullRequest(
        [FromRoute] string owner,
        [FromRoute] string name,
        [FromRoute] int number,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender
            .Send(new AnalyzePullRequestQuery(owner, name, number), cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(result);
    }
}
