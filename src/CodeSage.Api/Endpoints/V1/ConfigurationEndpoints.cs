using CodeSage.Application.Configuration;
using CodeSage.Contracts.Configuration;
using CodeSage.Infrastructure.Options;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CodeSage.Api.Endpoints.V1;

/// <summary>
/// Public configuration summary — never exposes secrets.
/// </summary>
public static class ConfigurationEndpoints
{
    public static RouteGroupBuilder MapConfigurationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/configuration", GetConfiguration)
            .WithName("GetConfigurationV1")
            .WithTags("Configuration")
            .WithSummary("Public configuration summary")
            .WithDescription(
                "Returns non-secret runtime configuration useful for clients and troubleshooting. "
                + "API keys and tokens are never included — only whether they are configured.")
            .Produces<ConfigurationSummaryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static Ok<ConfigurationSummaryResponse> GetConfiguration(
        IOptions<ApplicationOptions> applicationOptions,
        IOptions<GitHubOptions> gitHubOptions,
        IOptions<OpenAiOptions> openAiOptions,
        IHostEnvironment hostEnvironment)
    {
        var app = applicationOptions.Value;
        var github = gitHubOptions.Value;
        var openAi = openAiOptions.Value;

        var environment = string.IsNullOrWhiteSpace(app.Environment)
            ? hostEnvironment.EnvironmentName
            : app.Environment;

        return TypedResults.Ok(new ConfigurationSummaryResponse(
            Application: app.Name,
            Version: app.Version,
            Environment: environment,
            GitHubApiBaseUrl: github.ApiBaseUrl,
            GitHubTokenConfigured: github.HasPersonalAccessToken,
            AiProvider: "OpenAI",
            AiModel: openAi.Model,
            OpenAiBaseUrl: openAi.BaseUrl,
            OpenAiApiKeyConfigured: openAi.HasApiKey,
            ProbeExternalConnectivity: app.ProbeExternalConnectivity,
            RequireSecretsAtStartup: app.RequireSecretsAtStartup));
    }
}
