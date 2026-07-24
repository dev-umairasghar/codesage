using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.Analysis.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSage.Application.Features.AI.Services;

/// <summary>
/// Orchestrates prompt building, provider invocation, and response parsing.
/// Provider-agnostic — swapping OpenAI for Anthropic requires no changes here.
/// </summary>
public sealed class AiReviewService(
    IPromptBuilder promptBuilder,
    IAIProvider aiProvider,
    IReviewParser reviewParser,
    IOptions<AiReviewOptions> options,
    ILogger<AiReviewService> logger) : IAIReviewService
{
    /// <inheritdoc />
    public async Task<ReviewReport> ReviewAsync(
        ReviewContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        logger.LogInformation(
            "Generating AI review for {Repository} PR #{Number} via {Provider}",
            context.Repository.FullName,
            context.PullRequest.Number,
            aiProvider.Name);

        var prompt = promptBuilder.Build(context);

        if (options.Value.LogPrompts)
        {
            // Explicit opt-in — prompts may contain private source code.
            logger.LogDebug(
                "AI prompt generated. SystemLength={SystemLength}, DeveloperLength={DeveloperLength}, UserLength={UserLength}. System={System}. Developer={Developer}. User={User}",
                prompt.SystemPrompt.Length,
                prompt.DeveloperPrompt.Length,
                prompt.UserPrompt.Length,
                prompt.SystemPrompt,
                prompt.DeveloperPrompt,
                prompt.UserPrompt);
        }
        else
        {
            logger.LogInformation(
                "AI prompt generated. SystemLength={SystemLength}, DeveloperLength={DeveloperLength}, UserLength={UserLength}",
                prompt.SystemPrompt.Length,
                prompt.DeveloperPrompt.Length,
                prompt.UserPrompt.Length);
        }

        var completion = await aiProvider
            .CompleteAsync(new AiCompletionRequest(prompt), cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "AI completion received. Provider={Provider}, Model={Model}, DurationMs={DurationMs}, PromptTokens={PromptTokens}, CompletionTokens={CompletionTokens}, TotalTokens={TotalTokens}",
            aiProvider.Name,
            completion.Model,
            (int)completion.Duration.TotalMilliseconds,
            completion.PromptTokens,
            completion.CompletionTokens,
            completion.TotalTokens);

        var report = reviewParser.Parse(completion);

        logger.LogInformation(
            "AI review parsed. OverallRisk={OverallRisk}, Issues={IssueCount}, Security={SecurityCount}",
            report.OverallRisk,
            report.Issues.Count,
            report.SecurityConcerns.Count);

        return report;
    }
}
