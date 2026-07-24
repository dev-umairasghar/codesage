namespace CodeSage.Application.Features.AI;

/// <summary>
/// Cross-provider AI review options (prompt logging, patch budget).
/// Bound from the <c>AI</c> configuration section.
/// </summary>
public sealed class AiReviewOptions
{
    public const string SectionName = "AI";

    /// <summary>
    /// When true, logs prompt role contents at Debug. Never enable in shared production logs.
    /// </summary>
    public bool LogPrompts { get; set; }

    /// <summary>
    /// Maximum characters of patch text included per file in the user prompt.
    /// </summary>
    public int MaxPatchCharactersPerFile { get; set; } = 4000;

    /// <summary>
    /// Maximum number of changed files whose patches are included.
    /// </summary>
    public int MaxFilesWithPatches { get; set; } = 40;
}
