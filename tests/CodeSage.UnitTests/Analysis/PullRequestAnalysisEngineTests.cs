using CodeSage.Application.Features.Analysis;
using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;

namespace CodeSage.UnitTests.Analysis;

public sealed class PullRequestAnalysisEngineTests
{
    private readonly IPullRequestAnalysisEngine _sut = CreateEngine();

    [Fact]
    public void Analyze_BuildsCompleteReviewContext()
    {
        var input = new PullRequestAnalysisInput(
            RepositoryName: "Healthcare.API",
            RepositoryFullName: "acme/Healthcare.API",
            RepositoryDefaultBranch: "main",
            PullRequestNumber: 42,
            Title: "Improve patient lookup",
            Description: "Adds SQL and service changes.",
            State: "open",
            IsDraft: false,
            AuthorLogin: "octocat",
            AuthorDisplayName: "Octo Cat",
            AuthorAvatarUrl: "https://example.com/a.png",
            BaseRef: "main",
            HeadRef: "feature/patients",
            CreatedAt: DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
            UpdatedAt: DateTimeOffset.Parse("2024-01-02T00:00:00Z"),
            Commits:
            [
                new CommitAnalysisInput("abc123", "add service", "Octo", "octocat", DateTimeOffset.Parse("2024-01-01T01:00:00Z"))
            ],
            ChangedFiles:
            [
                new("src/Controllers/PatientsController.cs", "modified", 10, 2, 12, "@@ controller"),
                new("src/Services/PatientService.cs", "modified", 30, 4, 34, "@@ service"),
                new("db/GetPatient.sql", "added", 20, 0, 20, "@@ sql"),
                new("appsettings.json", "modified", 1, 0, 1, "@@ json"),
                new("tests/PatientServiceTests.cs", "added", 15, 0, 15, "@@ test"),
                new("docs/readme.md", "modified", 3, 1, 4, "@@ md"),
                new("assets/logo.png", "added", 0, 0, 0, "binary-data")
            ]);

        var context = _sut.Analyze(input);

        context.Repository.Name.Should().Be("Healthcare.API");
        context.PullRequest.Number.Should().Be(42);
        context.PullRequest.Title.Should().Be("Improve patient lookup");
        context.Author.Login.Should().Be("octocat");
        context.Commits.Should().ContainSingle(commit => commit.Sha == "abc123");

        context.ChangedFiles.Should().HaveCount(7);
        context.ChangedFiles.Should().Contain(file =>
            file.Filename.EndsWith("PatientService.cs")
            && file.Language == CodeLanguage.CSharp
            && file.Status == FileChangeStatus.Modified);

        context.ChangedFiles.Should().Contain(file =>
            file.Filename.EndsWith("logo.png")
            && file.IsBinary
            && file.Patch == null);

        context.Statistics.FileCount.Should().Be(7);
        context.Statistics.CommitCount.Should().Be(1);
        context.Statistics.SqlModified.Should().BeTrue();
        context.Statistics.ControllerFilesChanged.Should().Be(1);
        context.Statistics.ServiceFilesChanged.Should().Be(1);
        context.Statistics.TestFilesChanged.Should().Be(1);
        context.Statistics.LargestModifiedFile.Should().Be("src/Services/PatientService.cs");

        context.LanguageBreakdown.Keys.Should().Contain(["C#", "SQL", "JSON", "Markdown"]);
        context.Summary.Should().Contain("Healthcare.API");
        context.Summary.Should().Contain("SQL Modified:");
        context.Summary.Should().Contain("Yes");
    }

    [Fact]
    public void Analyze_IgnoresBinaryInLanguageBreakdown()
    {
        var input = new PullRequestAnalysisInput(
            "Repo",
            "org/Repo",
            "main",
            1,
            "t",
            null,
            "open",
            false,
            "dev",
            null,
            null,
            "main",
            "f",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            [],
            [
                new("a.cs", "modified", 1, 0, 1, "x"),
                new("b.png", "added", 0, 0, 0, null)
            ]);

        var context = _sut.Analyze(input);

        context.LanguageBreakdown.Should().ContainKey("C#");
        context.LanguageBreakdown.Keys.Should().NotContain("Binary");
    }

    private static IPullRequestAnalysisEngine CreateEngine()
    {
        var languageAnalyzer = new LanguageAnalyzer();
        var fileAnalyzer = new FileAnalyzer(languageAnalyzer);
        var statisticsAnalyzer = new StatisticsAnalyzer();
        var summaryBuilder = new SummaryBuilder();
        var builder = new ReviewContextBuilder(fileAnalyzer, statisticsAnalyzer, summaryBuilder);
        return new PullRequestAnalysisEngine(builder);
    }
}
