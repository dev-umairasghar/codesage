using CodeSage.Application.Features.AI;
using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Services;
using CodeSage.Application.Features.Analysis;
using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Behaviors;
using CodeSage.Application.Configuration;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CodeSage.Application;

/// <summary>
/// Application-layer composition root helpers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application services: CQRS, analysis engine, and AI review orchestration.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        if (configuration is not null)
        {
            services
                .AddOptions<ApplicationOptions>()
                .Bind(configuration.GetSection(ApplicationOptions.SectionName))
                .ValidateOnStart();
            services.AddSingleton<IValidateOptions<ApplicationOptions>, ApplicationOptionsValidator>();

            services
                .AddOptions<AiReviewOptions>()
                .Bind(configuration.GetSection(AiReviewOptions.SectionName))
                .Validate(
                    options => options.MaxPatchCharactersPerFile > 0,
                    "AI:MaxPatchCharactersPerFile must be positive.")
                .Validate(
                    options => options.MaxFilesWithPatches > 0,
                    "AI:MaxFilesWithPatches must be positive.")
                .ValidateOnStart();
        }
        else
        {
            services.AddOptions<ApplicationOptions>();
            services.AddOptions<AiReviewOptions>();
        }

        services.AddSingleton<ILanguageAnalyzer, LanguageAnalyzer>();
        services.AddSingleton<IFileAnalyzer, FileAnalyzer>();
        services.AddSingleton<IStatisticsAnalyzer, StatisticsAnalyzer>();
        services.AddSingleton<ISummaryBuilder, SummaryBuilder>();
        services.AddSingleton<IReviewContextBuilder, ReviewContextBuilder>();
        services.AddSingleton<IPullRequestAnalysisEngine, PullRequestAnalysisEngine>();

        services.AddSingleton<IPromptBuilder, PromptBuilder>();
        services.AddSingleton<IReviewParser, ReviewParser>();
        services.AddScoped<IAIReviewService, AiReviewService>();

        return services;
    }
}
