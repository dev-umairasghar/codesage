using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.AI.Abstractions;

/// <summary>
/// Outbound port for any LLM provider (OpenAI, Azure OpenAI, Anthropic, Ollama, …).
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Provider name for logging and diagnostics (e.g. "OpenAI").
    /// </summary>
    string Name { get; }

    Task<AiCompletionResult> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds maintainable multi-role prompts from <see cref="ReviewContext"/>.
/// </summary>
public interface IPromptBuilder
{
    AiPrompt Build(ReviewContext context);
}

/// <summary>
/// Parses raw LLM text into a strongly typed <see cref="ReviewReport"/>.
/// </summary>
public interface IReviewParser
{
    ReviewReport Parse(AiCompletionResult completion);
}

/// <summary>
/// Application orchestration: ReviewContext → prompt → provider → report.
/// </summary>
public interface IAIReviewService
{
    Task<ReviewReport> ReviewAsync(
        ReviewContext context,
        CancellationToken cancellationToken = default);
}
