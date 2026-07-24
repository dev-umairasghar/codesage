using CodeSage.Application.Features.AI.Commands;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.Analysis.Models;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CodeSage.Api.Endpoints.V1;

/// <summary>
/// Stateless AI review endpoints (API v1).
/// </summary>
public static class ReviewEndpoints
{
    public static RouteGroupBuilder MapReviewEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/reviews", CreateReview)
            .WithName("CreateReviewV1")
            .WithTags("Reviews")
            .WithSummary("Create AI review")
            .WithDescription(
                "Runs the configured OpenAI model against a ReviewContext and returns a structured "
                + "ReviewReport. Stateless — nothing is persisted. "
                + "Obtain a ReviewContext from GET .../pull-requests/{number}/analysis.")
            .Accepts<ReviewContext>("application/json")
            .Produces<ReviewReport>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status502BadGateway)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout);

        return group;
    }

    private static async Task<Ok<ReviewReport>> CreateReview(
        [FromBody] ReviewContext context,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var report = await sender
            .Send(new CreateReviewCommand(context), cancellationToken)
            .ConfigureAwait(false);
        return TypedResults.Ok(report);
    }
}
