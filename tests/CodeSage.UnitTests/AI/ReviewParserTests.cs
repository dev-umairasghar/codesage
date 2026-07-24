using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.AI.Services;
using FluentAssertions;

namespace CodeSage.UnitTests.AI;

public sealed class ReviewParserTests
{
    private readonly ReviewParser _sut = new();

    [Fact]
    public void Parse_DeserializesStructuredJson()
    {
        var json =
            """
            {
              "reviewSummary": "Solid change with one risk.",
              "overallRisk": "Medium",
              "positiveFindings": ["Clear naming"],
              "issues": [
                {
                  "title": "Missing null check",
                  "description": "Service assumes patient is non-null.",
                  "whyItMatters": "Can throw NullReferenceException in production.",
                  "severity": "High",
                  "category": "BugRisk",
                  "filePath": "src/Services/PatientService.cs",
                  "startLine": 42,
                  "endLine": 48,
                  "suggestion": "Add an ArgumentNullException guard."
                }
              ],
              "recommendations": ["Add integration tests"],
              "missingTests": ["Null patient path"],
              "securityConcerns": [],
              "performanceConcerns": [],
              "maintainability": [],
              "architectureConcerns": []
            }
            """;

        var report = _sut.Parse(new AiCompletionResult(
            json,
            "gpt-4o-mini",
            100,
            50,
            150,
            TimeSpan.FromMilliseconds(250)));

        report.Summary.Should().Be("Solid change with one risk.");
        report.OverallRisk.Should().Be(ReviewRiskLevel.Medium);
        report.PositiveFindings.Should().ContainSingle("Clear naming");
        report.Issues.Should().ContainSingle();
        report.Issues[0].Title.Should().Be("Missing null check");
        report.Issues[0].WhyItMatters.Should().Contain("NullReferenceException");
        report.Issues[0].Severity.Should().Be(FindingSeverity.High);
        report.Issues[0].Category.Should().Be(FindingCategory.BugRisk);
        report.Issues[0].FilePath.Should().Be("src/Services/PatientService.cs");
        report.Model.Should().Be("gpt-4o-mini");
        report.TotalTokens.Should().Be(150);
    }

    [Fact]
    public void Parse_AcceptsMarkdownFencedJson()
    {
        var content =
            """
            ```json
            {"reviewSummary":"ok","overallRisk":"Low","positiveFindings":[],"issues":[],"recommendations":[],"missingTests":[],"securityConcerns":[],"performanceConcerns":[],"maintainability":[],"architectureConcerns":[]}
            ```
            """;

        var report = _sut.Parse(Completion(content));

        report.OverallRisk.Should().Be(ReviewRiskLevel.Low);
        report.Summary.Should().Be("ok");
    }

    [Fact]
    public void Parse_EmptyContent_ThrowsAiInvalidResponseException()
    {
        var act = () => _sut.Parse(Completion("   "));

        act.Should().Throw<AiInvalidResponseException>();
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsAiInvalidResponseException()
    {
        var act = () => _sut.Parse(Completion("{not-json"));

        act.Should().Throw<AiInvalidResponseException>();
    }

    private static AiCompletionResult Completion(string content) =>
        new(content, "gpt-test", null, null, null, TimeSpan.FromMilliseconds(1));
}
