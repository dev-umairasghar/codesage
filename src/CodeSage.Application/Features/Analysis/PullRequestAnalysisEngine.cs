using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.Analysis;

/// <summary>
/// Application entry point for PR → ReviewContext transformation.
/// Future AI providers depend on this output only — never on GitHub types.
/// </summary>
public interface IPullRequestAnalysisEngine
{
    ReviewContext Analyze(PullRequestAnalysisInput input);
}

/// <inheritdoc />
public sealed class PullRequestAnalysisEngine(IReviewContextBuilder reviewContextBuilder)
    : IPullRequestAnalysisEngine
{
    /// <inheritdoc />
    public ReviewContext Analyze(PullRequestAnalysisInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return reviewContextBuilder.Build(input);
    }
}
