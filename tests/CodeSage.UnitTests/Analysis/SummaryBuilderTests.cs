using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;

namespace CodeSage.UnitTests.Analysis;

public sealed class SummaryBuilderTests
{
    private readonly SummaryBuilder _sut = new();

    [Fact]
    public void Build_ProducesDeterministicSummary()
    {
        var input = CreateInput("Healthcare.API");
        var statistics = new ReviewStatistics(
            FileCount: 12,
            CommitCount: 3,
            Additions: 100,
            Deletions: 20,
            TotalChangedLines: 120,
            LanguagesUsed: ["C#", "SQL"],
            LargestModifiedFile: "PatientService.cs",
            LargestModifiedFileChanges: 50,
            TestFilesChanged: 0,
            SqlFilesChanged: 1,
            ConfigurationFilesChanged: 1,
            ControllerFilesChanged: 2,
            ServiceFilesChanged: 3,
            SqlModified: true);

        var summary = _sut.Build(input, statistics, new Dictionary<string, int>
        {
            ["C#"] = 10,
            ["SQL"] = 2
        });

        summary.Should().Be(
            """
            Repository:
            Healthcare.API

            Changed Files:
            12

            Languages:
            C#
            SQL

            Largest File:
            PatientService.cs

            SQL Modified:
            Yes

            Controllers Changed:
            2

            Services Changed:
            3

            Tests Changed:
            0
            """.ReplaceLineEndings());
    }

    private static PullRequestAnalysisInput CreateInput(string repositoryName) =>
        new(
            repositoryName,
            $"{repositoryName}/{repositoryName}",
            "main",
            1,
            "title",
            "body",
            "open",
            false,
            "author",
            null,
            null,
            "main",
            "feature",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [],
            []);
}
