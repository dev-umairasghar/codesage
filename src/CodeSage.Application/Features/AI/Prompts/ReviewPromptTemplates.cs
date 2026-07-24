namespace CodeSage.Application.Features.AI.Prompts;

/// <summary>
/// Static prompt templates — easy to revise without touching orchestration code.
/// Keep JSON schema instructions here so the parser contract stays stable.
/// </summary>
internal static class ReviewPromptTemplates
{
    public const string SystemPrompt =
        """
        You are CodeSage, an expert software engineer performing pull request reviews.
        You review code for quality, maintainability, readability, architecture, performance,
        security, error handling, naming, potential bugs, missing tests, and regressions.
        You explain WHY each issue matters.
        You respond with JSON only. Never use Markdown. Never wrap JSON in code fences.
        """;

    public const string DeveloperPrompt =
        """
        Return a single JSON object with exactly these properties:
        {
          "reviewSummary": "string",
          "overallRisk": "Low | Medium | High | Critical",
          "positiveFindings": ["string"],
          "issues": [Finding],
          "recommendations": ["string"],
          "missingTests": ["string"],
          "securityConcerns": [Finding],
          "performanceConcerns": [Finding],
          "maintainability": [Finding],
          "architectureConcerns": [Finding]
        }

        Finding object shape:
        {
          "title": "string",
          "description": "string",
          "whyItMatters": "string",
          "severity": "Info | Low | Medium | High | Critical",
          "category": "CodeQuality | Maintainability | Readability | Architecture | Performance | Security | ErrorHandling | Naming | BugRisk | MissingTests | RegressionRisk | Other",
          "filePath": "string or null",
          "startLine": number or null,
          "endLine": number or null,
          "suggestion": "string or null"
        }

        Rules:
        - Be specific and actionable.
        - Prefer findings grounded in the provided diffs.
        - If there are no items for a list, return an empty array.
        - overallRisk must reflect the worst credible issue severity.
        """;

    public const string UserPromptHeader =
        """
        Review the following pull request context and produce the JSON review report.
        """;
}
