using System.Net;
using System.Net.Http.Headers;
using CodeSage.Application.Configuration;
using CodeSage.Application.Interfaces;
using CodeSage.Contracts.Health;
using CodeSage.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSage.Infrastructure.Diagnostics;

/// <summary>
/// Assembles configuration + optional connectivity diagnostics for local troubleshooting.
/// Never returns API keys or tokens.
/// </summary>
public sealed class SystemStatusService(
    IOptions<ApplicationOptions> applicationOptions,
    IOptions<GitHubOptions> gitHubOptions,
    IOptions<OpenAiOptions> openAiOptions,
    IHttpClientFactory httpClientFactory,
    IHostEnvironment hostEnvironment,
    ILogger<SystemStatusService> logger) : ISystemStatusService
{
    public async Task<SystemStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var app = applicationOptions.Value;
        var github = gitHubOptions.Value;
        var openAi = openAiOptions.Value;

        var environment = string.IsNullOrWhiteSpace(app.Environment)
            ? hostEnvironment.EnvironmentName
            : app.Environment;

        var diagnostics = new List<string>();

        if (!github.HasPersonalAccessToken)
        {
            diagnostics.Add("GitHub Personal Access Token is not configured.");
        }

        if (!openAi.HasApiKey)
        {
            diagnostics.Add("OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(openAi.Model))
        {
            diagnostics.Add("OpenAI model is not configured.");
        }

        ConnectivityCheckResult githubConnectivity;
        ConnectivityCheckResult openAiConnectivity;

        if (!app.ProbeExternalConnectivity)
        {
            githubConnectivity = new ConnectivityCheckResult(
                "Skipped",
                "External probes disabled (Application:ProbeExternalConnectivity=false).");
            openAiConnectivity = githubConnectivity;
        }
        else
        {
            githubConnectivity = await ProbeGitHubAsync(github, cancellationToken).ConfigureAwait(false);
            openAiConnectivity = await ProbeOpenAiAsync(openAi, cancellationToken).ConfigureAwait(false);
        }

        if (githubConnectivity.Status is "Unhealthy" or "Misconfigured")
        {
            diagnostics.Add($"GitHub: {githubConnectivity.Message}");
        }

        if (openAiConnectivity.Status is "Unhealthy" or "Misconfigured")
        {
            diagnostics.Add($"OpenAI: {openAiConnectivity.Message}");
        }

        if (diagnostics.Count == 0)
        {
            diagnostics.Add("Configuration looks ready for local reviews.");
        }

        return new SystemStatusResponse(
            Application: app.Name,
            Version: app.Version,
            Environment: environment,
            AiProvider: "OpenAI",
            AiModel: openAi.Model,
            GitHubTokenConfigured: github.HasPersonalAccessToken,
            OpenAiApiKeyConfigured: openAi.HasApiKey,
            GitHubConnectivity: githubConnectivity,
            OpenAiConnectivity: openAiConnectivity,
            Diagnostics: diagnostics);
    }

    private async Task<ConnectivityCheckResult> ProbeGitHubAsync(
        GitHubOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.HasPersonalAccessToken)
        {
            return new ConnectivityCheckResult(
                "Misconfigured",
                "Missing GitHub:PersonalAccessToken. See docs/Configuration.md.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("GitHubProbe");
            using var request = new HttpRequestMessage(HttpMethod.Get, "user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.PersonalAccessToken);
            request.Headers.UserAgent.ParseAdd(options.UserAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return new ConnectivityCheckResult(
                    "Healthy",
                    "GitHub API accepted the configured token.",
                    (int)response.StatusCode);
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new ConnectivityCheckResult(
                    "Unhealthy",
                    "GitHub rejected the token (unauthorized/forbidden). Check scopes and expiry.",
                    (int)response.StatusCode);
            }

            return new ConnectivityCheckResult(
                "Unhealthy",
                $"GitHub API returned {(int)response.StatusCode} {response.ReasonPhrase}.",
                (int)response.StatusCode);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "GitHub connectivity probe failed");
            return new ConnectivityCheckResult(
                "Unhealthy",
                "Could not reach GitHub API. Check network and GitHub:ApiBaseUrl.");
        }
    }

    private async Task<ConnectivityCheckResult> ProbeOpenAiAsync(
        OpenAiOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.HasApiKey)
        {
            return new ConnectivityCheckResult(
                "Misconfigured",
                "Missing OpenAI:ApiKey. See docs/Configuration.md.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("OpenAiProbe");
            using var request = new HttpRequestMessage(HttpMethod.Get, "models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return new ConnectivityCheckResult(
                    "Healthy",
                    "OpenAI API accepted the configured key.",
                    (int)response.StatusCode);
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new ConnectivityCheckResult(
                    "Unhealthy",
                    "OpenAI rejected the API key (unauthorized/forbidden).",
                    (int)response.StatusCode);
            }

            return new ConnectivityCheckResult(
                "Unhealthy",
                $"OpenAI API returned {(int)response.StatusCode} {response.ReasonPhrase}.",
                (int)response.StatusCode);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "OpenAI connectivity probe failed");
            return new ConnectivityCheckResult(
                "Unhealthy",
                "Could not reach OpenAI API. Check network and OpenAI:BaseUrl.");
        }
    }
}
