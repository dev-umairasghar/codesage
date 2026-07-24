using CodeSage.Application.Features.Analysis.Models;

namespace CodeSage.Application.Features.Analysis.Analyzers;

/// <summary>
/// Detects language from filename / extension.
/// Kept as an interface so Stage 4+ can plug richer detectors (shebang, content sniffing).
/// </summary>
public interface ILanguageAnalyzer
{
    CodeLanguage DetectLanguage(string filename);

    string GetExtension(string filename);

    bool IsBinary(string filename, string? patch);
}

/// <inheritdoc />
public sealed class LanguageAnalyzer : ILanguageAnalyzer
{
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp", ".svg",
        ".pdf", ".zip", ".gz", ".tar", ".7z", ".rar",
        ".dll", ".exe", ".pdb", ".so", ".dylib",
        ".woff", ".woff2", ".ttf", ".eot",
        ".mp3", ".mp4", ".wav", ".avi", ".mov",
        ".nupkg", ".snupkg"
    };

    /// <inheritdoc />
    public string GetExtension(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return string.Empty;
        }

        return Path.GetExtension(filename.Replace('\\', '/'));
    }

    /// <inheritdoc />
    public bool IsBinary(string filename, string? patch)
    {
        var extension = GetExtension(filename);
        if (BinaryExtensions.Contains(extension))
        {
            return true;
        }

        // Git often omits patches for binary blobs.
        if (patch is null
            && !string.IsNullOrWhiteSpace(extension)
            && DetectLanguage(filename) == CodeLanguage.Unknown)
        {
            // Conservative: unknown extension without a patch is treated as non-supported text, not binary,
            // unless the extension is in the binary list above.
            return false;
        }

        if (patch is not null
            && patch.Contains("GIT binary patch", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public CodeLanguage DetectLanguage(string filename)
    {
        var extension = GetExtension(filename);
        if (string.IsNullOrEmpty(extension))
        {
            return CodeLanguage.Unknown;
        }

        if (BinaryExtensions.Contains(extension))
        {
            return CodeLanguage.Binary;
        }

        return extension.ToLowerInvariant() switch
        {
            ".cs" => CodeLanguage.CSharp,
            ".sql" => CodeLanguage.Sql,
            ".js" or ".mjs" or ".cjs" => CodeLanguage.JavaScript,
            ".ts" or ".tsx" => CodeLanguage.TypeScript,
            ".json" => CodeLanguage.Json,
            ".yml" or ".yaml" => CodeLanguage.Yaml,
            ".md" or ".markdown" => CodeLanguage.Markdown,
            ".xml" or ".csproj" or ".fsproj" or ".vbproj" or ".props" or ".targets"
                or ".config" or ".resx" => CodeLanguage.Xml,
            _ => CodeLanguage.Unknown
        };
    }
}
