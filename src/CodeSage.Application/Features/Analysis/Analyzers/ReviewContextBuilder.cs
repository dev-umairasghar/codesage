using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.Analysis.Analyzers;

/// <summary>
/// Orchestrates analyzers into a complete <see cref="ReviewContext"/>.
/// </summary>
public interface IReviewContextBuilder
{
    ReviewContext Build(PullRequestAnalysisInput input);
}

/// <inheritdoc />
public sealed class ReviewContextBuilder(
    IFileAnalyzer fileAnalyzer,
    IStatisticsAnalyzer statisticsAnalyzer,
    ISummaryBuilder summaryBuilder) : IReviewContextBuilder
{
    /// <inheritdoc />
    public ReviewContext Build(PullRequestAnalysisInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Binary files are still listed (for metrics) but patches are stripped by FileAnalyzer.
        var changedFiles = input.ChangedFiles
            .Select(fileAnalyzer.Analyze)
            .OrderBy(file => file.Filename, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var statistics = statisticsAnalyzer.Analyze(changedFiles, input.Commits);

        var languageBreakdown = changedFiles
            .Where(file => file.Language is not CodeLanguage.Unknown and not CodeLanguage.Binary)
            .GroupBy(file => StatisticsAnalyzer.ToDisplayName(file.Language))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var summary = summaryBuilder.Build(input, statistics, languageBreakdown);

        return new ReviewContext(
            Repository: new ReviewRepositoryInfo(
                input.RepositoryName,
                input.RepositoryFullName,
                input.RepositoryDefaultBranch),
            PullRequest: new ReviewPullRequestInfo(
                input.PullRequestNumber,
                input.Title,
                input.Description,
                input.State,
                input.IsDraft,
                input.BaseRef,
                input.HeadRef,
                input.CreatedAt,
                input.UpdatedAt),
            Author: new ReviewAuthorInfo(
                input.AuthorLogin,
                input.AuthorDisplayName,
                input.AuthorAvatarUrl),
            Commits: input.Commits
                .Select(commit => new ReviewCommitInfo(
                    commit.Sha,
                    commit.Message,
                    commit.AuthorName,
                    commit.AuthorLogin,
                    commit.CommittedAt))
                .ToList(),
            ChangedFiles: changedFiles,
            Statistics: statistics,
            LanguageBreakdown: languageBreakdown,
            Summary: summary);
    }
}
