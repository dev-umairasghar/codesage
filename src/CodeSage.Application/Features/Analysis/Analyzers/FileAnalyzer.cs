using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.Analysis.Analyzers;

/// <summary>
/// Normalizes a single changed file into a <see cref="ReviewFileChange"/>.
/// </summary>
public interface IFileAnalyzer
{
    ReviewFileChange Analyze(ChangedFileAnalysisInput file);
}

/// <inheritdoc />
public sealed class FileAnalyzer(ILanguageAnalyzer languageAnalyzer) : IFileAnalyzer
{
    /// <inheritdoc />
    public ReviewFileChange Analyze(ChangedFileAnalysisInput file)
    {
        ArgumentNullException.ThrowIfNull(file);

        var filename = file.Filename.Replace('\\', '/');
        var extension = languageAnalyzer.GetExtension(filename);
        var isBinary = languageAnalyzer.IsBinary(filename, file.Patch);
        var language = isBinary
            ? CodeLanguage.Binary
            : languageAnalyzer.DetectLanguage(filename);

        var isSupported = language is not CodeLanguage.Unknown and not CodeLanguage.Binary;

        return new ReviewFileChange(
            Filename: filename,
            Extension: extension,
            Language: language,
            Status: MapStatus(file.RawStatus),
            Additions: Math.Max(0, file.Additions),
            Deletions: Math.Max(0, file.Deletions),
            TotalChanges: Math.Max(0, file.Changes > 0 ? file.Changes : file.Additions + file.Deletions),
            Patch: isBinary ? null : file.Patch,
            IsBinary: isBinary,
            IsSupported: isSupported);
    }

    /// <summary>
    /// Maps vendor-neutral / GitHub-like status strings into CodeSage status.
    /// </summary>
    internal static FileChangeStatus MapStatus(string? rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return FileChangeStatus.Unknown;
        }

        return rawStatus.Trim().ToLowerInvariant() switch
        {
            "added" or "add" or "new" or "created" => FileChangeStatus.New,
            "modified" or "changed" or "edit" or "edited" => FileChangeStatus.Modified,
            "removed" or "deleted" or "delete" or "remove" => FileChangeStatus.Deleted,
            "renamed" or "rename" or "copied" or "copy" => FileChangeStatus.Renamed,
            _ => FileChangeStatus.Unknown
        };
    }
}
