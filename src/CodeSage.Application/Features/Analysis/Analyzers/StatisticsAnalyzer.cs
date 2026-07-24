using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.Analysis.Analyzers;

/// <summary>
/// Computes aggregate metrics over analyzed files and commits.
/// </summary>
public interface IStatisticsAnalyzer
{
    ReviewStatistics Analyze(
        IReadOnlyList<ReviewFileChange> files,
        IReadOnlyList<CommitAnalysisInput> commits);
}

/// <inheritdoc />
public sealed class StatisticsAnalyzer : IStatisticsAnalyzer
{
    /// <inheritdoc />
    public ReviewStatistics Analyze(
        IReadOnlyList<ReviewFileChange> files,
        IReadOnlyList<CommitAnalysisInput> commits)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(commits);

        var additions = files.Sum(file => file.Additions);
        var deletions = files.Sum(file => file.Deletions);

        var languagesUsed = files
            .Where(file => file.Language is not CodeLanguage.Unknown and not CodeLanguage.Binary)
            .Select(file => ToDisplayName(file.Language))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var largest = files
            .Where(file => !file.IsBinary)
            .OrderByDescending(file => file.TotalChanges)
            .ThenBy(file => file.Filename, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        return new ReviewStatistics(
            FileCount: files.Count,
            CommitCount: commits.Count,
            Additions: additions,
            Deletions: deletions,
            TotalChangedLines: additions + deletions,
            LanguagesUsed: languagesUsed,
            LargestModifiedFile: largest?.Filename,
            LargestModifiedFileChanges: largest?.TotalChanges ?? 0,
            TestFilesChanged: files.Count(IsTestFile),
            SqlFilesChanged: files.Count(file => file.Language == CodeLanguage.Sql),
            ConfigurationFilesChanged: files.Count(IsConfigurationFile),
            ControllerFilesChanged: files.Count(IsControllerFile),
            ServiceFilesChanged: files.Count(IsServiceFile),
            SqlModified: files.Any(file => file.Language == CodeLanguage.Sql));
    }

    internal static string ToDisplayName(CodeLanguage language) => language switch
    {
        CodeLanguage.CSharp => "C#",
        CodeLanguage.Sql => "SQL",
        CodeLanguage.JavaScript => "JavaScript",
        CodeLanguage.TypeScript => "TypeScript",
        CodeLanguage.Json => "JSON",
        CodeLanguage.Yaml => "YAML",
        CodeLanguage.Markdown => "Markdown",
        CodeLanguage.Xml => "XML",
        CodeLanguage.Binary => "Binary",
        _ => "Unknown"
    };

    internal static bool IsTestFile(ReviewFileChange file)
    {
        var path = file.Filename.Replace('\\', '/');
        var name = Path.GetFileName(path);

        return path.StartsWith("tests/", StringComparison.OrdinalIgnoreCase)
               || path.StartsWith("test/", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/tests/", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/test/", StringComparison.OrdinalIgnoreCase)
               || path.Contains(".tests/", StringComparison.OrdinalIgnoreCase)
               || path.Contains(".test/", StringComparison.OrdinalIgnoreCase)
               || name.Contains("Test", StringComparison.OrdinalIgnoreCase)
               || name.Contains("Spec", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith("_test.cs", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".test.ts", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".test.js", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsConfigurationFile(ReviewFileChange file)
    {
        if (file.Language is CodeLanguage.Json or CodeLanguage.Yaml or CodeLanguage.Xml)
        {
            return true;
        }

        var name = Path.GetFileName(file.Filename);
        return name.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase)
               || name.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)
               || name.Equals(".env", StringComparison.OrdinalIgnoreCase)
               || name.EndsWith(".config", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsControllerFile(ReviewFileChange file)
    {
        var name = Path.GetFileNameWithoutExtension(file.Filename);
        return name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
               || file.Filename.Contains("/Controllers/", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsServiceFile(ReviewFileChange file)
    {
        var name = Path.GetFileNameWithoutExtension(file.Filename);
        return name.EndsWith("Service", StringComparison.OrdinalIgnoreCase)
               || file.Filename.Contains("/Services/", StringComparison.OrdinalIgnoreCase);
    }
}
