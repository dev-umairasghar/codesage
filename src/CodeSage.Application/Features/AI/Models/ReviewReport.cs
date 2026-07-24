namespace CodeSage.Application.Features.AI.Models;

/// <summary>
/// Provider-agnostic chat prompt with explicit roles for maintainability.
/// </summary>
public sealed record AiPrompt(
    string SystemPrompt,
    string DeveloperPrompt,
    string UserPrompt);

/// <summary>
/// Request sent to any <see cref="Abstractions.IAIProvider"/> implementation.
/// </summary>
public sealed record AiCompletionRequest(
    AiPrompt Prompt,
    string? Model = null,
    double? Temperature = null,
    int? MaxTokens = null);

/// <summary>
/// Normalized completion result — no OpenAI SDK types.
/// </summary>
public sealed record AiCompletionResult(
    string Content,
    string Model,
    int? PromptTokens,
    int? CompletionTokens,
    int? TotalTokens,
    TimeSpan Duration);

/// <summary>
/// Overall risk level assigned by the model.
/// </summary>
public enum ReviewRiskLevel
{
    Unknown = 0,
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Severity for an individual finding.
/// </summary>
public enum FindingSeverity
{
    Unknown = 0,
    Info,
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Category for an individual finding.
/// </summary>
public enum FindingCategory
{
    Unknown = 0,
    CodeQuality,
    Maintainability,
    Readability,
    Architecture,
    Performance,
    Security,
    ErrorHandling,
    Naming,
    BugRisk,
    MissingTests,
    RegressionRisk,
    Other
}

/// <summary>
/// Structured review report — the only AI output the rest of CodeSage consumes.
/// </summary>
public sealed record ReviewReport(
    string Summary,
    ReviewRiskLevel OverallRisk,
    IReadOnlyList<string> PositiveFindings,
    IReadOnlyList<ReviewFinding> Issues,
    IReadOnlyList<string> Recommendations,
    IReadOnlyList<string> MissingTests,
    IReadOnlyList<ReviewFinding> SecurityConcerns,
    IReadOnlyList<ReviewFinding> PerformanceConcerns,
    IReadOnlyList<ReviewFinding> Maintainability,
    IReadOnlyList<ReviewFinding> ArchitectureConcerns,
    string Model,
    int? PromptTokens,
    int? CompletionTokens,
    int? TotalTokens,
    TimeSpan Duration);

public sealed record ReviewFinding(
    string Title,
    string Description,
    string WhyItMatters,
    FindingSeverity Severity,
    FindingCategory Category,
    string? FilePath,
    int? StartLine,
    int? EndLine,
    string? Suggestion);

/// <summary>
/// Wire schema expected from the LLM (JSON). Mapped into <see cref="ReviewReport"/>.
/// </summary>
internal sealed class LlmReviewResponseDocument
{
    public string? ReviewSummary { get; set; }
    public string? OverallRisk { get; set; }
    public List<string>? PositiveFindings { get; set; }
    public List<LlmFindingDocument>? Issues { get; set; }
    public List<string>? Recommendations { get; set; }
    public List<string>? MissingTests { get; set; }
    public List<LlmFindingDocument>? SecurityConcerns { get; set; }
    public List<LlmFindingDocument>? PerformanceConcerns { get; set; }
    public List<LlmFindingDocument>? Maintainability { get; set; }
    public List<LlmFindingDocument>? ArchitectureConcerns { get; set; }
}

internal sealed class LlmFindingDocument
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? WhyItMatters { get; set; }
    public string? Severity { get; set; }
    public string? Category { get; set; }
    public string? FilePath { get; set; }
    public int? StartLine { get; set; }
    public int? EndLine { get; set; }
    public string? Suggestion { get; set; }
}
