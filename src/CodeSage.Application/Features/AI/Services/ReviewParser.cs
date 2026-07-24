using System.Text.Json;
using System.Text.RegularExpressions;
using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Models;

namespace CodeSage.Application.Features.AI.Services;

/// <summary>
/// Deserializes LLM JSON into <see cref="ReviewReport"/>.
/// Tolerates accidental Markdown fences but rejects empty/invalid payloads.
/// </summary>
public sealed partial class ReviewParser : IReviewParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc />
    public ReviewReport Parse(AiCompletionResult completion)
    {
        ArgumentNullException.ThrowIfNull(completion);

        if (string.IsNullOrWhiteSpace(completion.Content))
        {
            throw new AiInvalidResponseException("The AI provider returned an empty response.");
        }

        var json = ExtractJson(completion.Content);

        LlmReviewResponseDocument document;
        try
        {
            document = JsonSerializer.Deserialize<LlmReviewResponseDocument>(json, SerializerOptions)
                       ?? throw new AiInvalidResponseException("The AI provider returned null JSON.");
        }
        catch (JsonException exception)
        {
            throw new AiInvalidResponseException(
                "The AI provider returned invalid JSON that could not be parsed.",
                exception);
        }

        return new ReviewReport(
            Summary: document.ReviewSummary?.Trim() ?? string.Empty,
            OverallRisk: ParseRisk(document.OverallRisk),
            PositiveFindings: document.PositiveFindings ?? [],
            Issues: MapFindings(document.Issues),
            Recommendations: document.Recommendations ?? [],
            MissingTests: document.MissingTests ?? [],
            SecurityConcerns: MapFindings(document.SecurityConcerns),
            PerformanceConcerns: MapFindings(document.PerformanceConcerns),
            Maintainability: MapFindings(document.Maintainability),
            ArchitectureConcerns: MapFindings(document.ArchitectureConcerns),
            Model: completion.Model,
            PromptTokens: completion.PromptTokens,
            CompletionTokens: completion.CompletionTokens,
            TotalTokens: completion.TotalTokens,
            Duration: completion.Duration);
    }

    private static IReadOnlyList<ReviewFinding> MapFindings(List<LlmFindingDocument>? findings) =>
        (findings ?? [])
        .Select(finding => new ReviewFinding(
            Title: finding.Title?.Trim() ?? "Untitled finding",
            Description: finding.Description?.Trim() ?? string.Empty,
            WhyItMatters: finding.WhyItMatters?.Trim() ?? string.Empty,
            Severity: ParseSeverity(finding.Severity),
            Category: ParseCategory(finding.Category),
            FilePath: finding.FilePath,
            StartLine: finding.StartLine,
            EndLine: finding.EndLine,
            Suggestion: finding.Suggestion))
        .ToList();

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        var fenceMatch = MarkdownFenceRegex().Match(trimmed);
        if (fenceMatch.Success)
        {
            return fenceMatch.Groups[1].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }

    private static ReviewRiskLevel ParseRisk(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "low" => ReviewRiskLevel.Low,
            "medium" => ReviewRiskLevel.Medium,
            "high" => ReviewRiskLevel.High,
            "critical" => ReviewRiskLevel.Critical,
            _ => ReviewRiskLevel.Unknown
        };

    private static FindingSeverity ParseSeverity(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "info" => FindingSeverity.Info,
            "low" => FindingSeverity.Low,
            "medium" => FindingSeverity.Medium,
            "high" => FindingSeverity.High,
            "critical" => FindingSeverity.Critical,
            _ => FindingSeverity.Unknown
        };

    private static FindingCategory ParseCategory(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "codequality" => FindingCategory.CodeQuality,
            "maintainability" => FindingCategory.Maintainability,
            "readability" => FindingCategory.Readability,
            "architecture" => FindingCategory.Architecture,
            "performance" => FindingCategory.Performance,
            "security" => FindingCategory.Security,
            "errorhandling" => FindingCategory.ErrorHandling,
            "naming" => FindingCategory.Naming,
            "bugrisk" => FindingCategory.BugRisk,
            "missingtests" => FindingCategory.MissingTests,
            "regressionrisk" => FindingCategory.RegressionRisk,
            "other" => FindingCategory.Other,
            _ => FindingCategory.Unknown
        };

    [GeneratedRegex(@"```(?:json)?\s*(.*?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex MarkdownFenceRegex();
}
