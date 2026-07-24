using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Interfaces;
using CodeSage.Application.Configuration;
using CodeSage.Infrastructure.AI;
using CodeSage.Infrastructure.Diagnostics;
using CodeSage.Infrastructure.GitHub;
using CodeSage.Infrastructure.Options;
using CodeSage.Infrastructure.Options.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace CodeSage.Infrastructure;

/// <summary>
/// Infrastructure composition root — GitHub, OpenAI, diagnostics. No database.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<GitHubOptions>()
            .Bind(configuration.GetSection(GitHubOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<GitHubOptions>, GitHubOptionsValidator>();

        services
            .AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<OpenAiOptions>, OpenAiOptionsValidator>();

        services.AddSingleton<IGitHubAccessTokenAccessor, ConfiguredGitHubTokenAccessor>();
        services.AddScoped<ISystemStatusService, SystemStatusService>();

        services.AddHttpClient<IGitHubClient, GitHubClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GitHubOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiBaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("GitHubProbe", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GitHubOptions>>().Value;
            client.BaseAddress = new Uri(options.ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IAIProvider, OpenAiProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddHttpClient("OpenAiProbe", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    /// <summary>
    /// Forces options validation and writes safe startup diagnostics (never logs secrets).
    /// </summary>
    public static void LogStartupDiagnostics(IServiceProvider services, ILogger logger)
    {
        logger.Information("Configuration loaded from appsettings, environment variables, and user-secrets");

        var app = services.GetRequiredService<IOptions<ApplicationOptions>>().Value;
        _ = services.GetRequiredService<IOptions<GitHubOptions>>().Value;
        _ = services.GetRequiredService<IOptions<OpenAiOptions>>().Value;

        logger.Information("Configuration validation succeeded");
        logger.Information(
            "Registered services: GitHub client, OpenAI provider, system status, MediatR handlers");
        logger.Information(
            "Application ready — {Application} {Version} ({Environment})",
            app.Name,
            app.Version,
            string.IsNullOrWhiteSpace(app.Environment)
                ? services.GetRequiredService<IHostEnvironment>().EnvironmentName
                : app.Environment);
    }

    public static void ConfigureSerilog(
        HostBuilderContext context,
        IServiceProvider services,
        LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "CodeSage");
    }
}
