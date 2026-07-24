using CodeSage.Api.Endpoints.V1;

namespace CodeSage.Api.Endpoints;

/// <summary>
/// Maps versioned public API surfaces. Add <c>MapCodeSageApiV2</c> alongside when introducing breaking changes.
/// </summary>
public static class ApiEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps CodeSage API v1 under <c>/api/v1</c>.
    /// Also maps an unversioned <c>/api/health</c> probe alias for load balancers.
    /// </summary>
    public static IEndpointRouteBuilder MapCodeSageApiV1(this IEndpointRouteBuilder endpoints)
    {
        var v1 = endpoints.MapGroup("/api/v1");

        v1.MapHealthEndpoints();
        v1.MapConfigurationEndpoints();
        v1.MapRepositoryEndpoints();
        v1.MapReviewEndpoints();

        endpoints.MapUnversionedHealthProbe();

        return endpoints;
    }
}
