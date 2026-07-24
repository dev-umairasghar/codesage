using CodeSage.Application.Configuration;
using CodeSage.Application.Interfaces;
using CodeSage.Contracts.Health;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace CodeSage.Api.Endpoints.V1;

/// <summary>
/// Health and diagnostics endpoints (API v1).
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps <c>GET /api/v1/health</c> and <c>GET /api/v1/system/status</c> onto an existing <c>/api/v1</c> group.
    /// </summary>
    public static RouteGroupBuilder MapHealthEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/health", GetHealth)
            .WithName("GetHealthV1")
            .WithTags("Health")
            .WithSummary("Liveness probe")
            .WithDescription(
                "Returns process liveness. Does not probe GitHub or OpenAI. "
                + "Safe for orchestrators and load balancers.")
            .Produces<HealthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/system/status", GetSystemStatus)
            .WithName("GetSystemStatusV1")
            .WithTags("Health")
            .WithSummary("System diagnostics")
            .WithDescription(
                "Returns configuration readiness and optional connectivity probes. "
                + "Never includes API keys or tokens. "
                + "Disable probes with Application:ProbeExternalConnectivity=false.")
            .Produces<SystemStatusResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    /// <summary>
    /// Unversioned alias used by probes that expect <c>/api/health</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapUnversionedHealthProbe(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", GetHealth)
            .WithName("GetHealth")
            .WithTags("Health")
            .WithSummary("Liveness probe (unversioned alias)")
            .WithDescription("Alias of GET /api/v1/health for load balancers and local scripts.")
            .Produces<HealthResponse>(StatusCodes.Status200OK)
            .ExcludeFromDescription();

        return endpoints;
    }

    private static Ok<HealthResponse> GetHealth(IOptions<ApplicationOptions> options)
    {
        var app = options.Value;
        return TypedResults.Ok(new HealthResponse(
            Status: "Healthy",
            Application: app.Name,
            Version: app.Version));
    }

    private static async Task<Ok<SystemStatusResponse>> GetSystemStatus(
        ISystemStatusService statusService,
        CancellationToken cancellationToken)
    {
        var status = await statusService.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(status);
    }
}
