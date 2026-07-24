using System.Text;
using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.Analysis.Analyzers;

/// <summary>
/// Builds a deterministic, human-readable summary — not AI-generated.
/// Stable formatting matters so future evals can snapshot-compare summaries.
/// </summary>
public interface ISummaryBuilder
{
    string Build(
        PullRequestAnalysisInput input,
        ReviewStatistics statistics,
        IReadOnlyDictionary<string, int> languageBreakdown);
}

/// <inheritdoc />
public sealed class SummaryBuilder : ISummaryBuilder
{
    /// <inheritdoc />
    public string Build(
        PullRequestAnalysisInput input,
        ReviewStatistics statistics,
        IReadOnlyDictionary<string, int> languageBreakdown)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(statistics);
        ArgumentNullException.ThrowIfNull(languageBreakdown);

        var languages = statistics.LanguagesUsed.Count == 0
            ? "(none)"
            : string.Join(Environment.NewLine, statistics.LanguagesUsed);

        var largestFile = string.IsNullOrWhiteSpace(statistics.LargestModifiedFile)
            ? "(none)"
            : statistics.LargestModifiedFile;

        var builder = new StringBuilder();
        builder.AppendLine($"Repository:");
        builder.AppendLine(input.RepositoryName);
        builder.AppendLine();
        builder.AppendLine("Changed Files:");
        builder.AppendLine(statistics.FileCount.ToString());
        builder.AppendLine();
        builder.AppendLine("Languages:");
        builder.AppendLine(languages);
        builder.AppendLine();
        builder.AppendLine("Largest File:");
        builder.AppendLine(largestFile);
        builder.AppendLine();
        builder.AppendLine("SQL Modified:");
        builder.AppendLine(statistics.SqlModified ? "Yes" : "No");
        builder.AppendLine();
        builder.AppendLine("Controllers Changed:");
        builder.AppendLine(statistics.ControllerFilesChanged.ToString());
        builder.AppendLine();
        builder.AppendLine("Services Changed:");
        builder.AppendLine(statistics.ServiceFilesChanged.ToString());
        builder.AppendLine();
        builder.AppendLine("Tests Changed:");
        builder.AppendLine(statistics.TestFilesChanged.ToString());

        return builder.ToString().TrimEnd();
    }
}
