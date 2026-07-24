using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;

namespace CodeSage.UnitTests.Analysis;

public sealed class StatisticsAnalyzerTests
{
    private readonly StatisticsAnalyzer _sut = new();

    [Fact]
    public void Analyze_ComputesExpectedMetrics()
    {
        var files = new List<ReviewFileChange>
        {
            File("src/Controllers/PatientsController.cs", CodeLanguage.CSharp, 20, 5),
            File("src/Services/PatientService.cs", CodeLanguage.CSharp, 40, 10),
            File("db/Patients.sql", CodeLanguage.Sql, 8, 1),
            File("appsettings.json", CodeLanguage.Json, 2, 0),
            File("tests/PatientServiceTests.cs", CodeLanguage.CSharp, 15, 0),
            File("assets/logo.png", CodeLanguage.Binary, 0, 0, isBinary: true)
        };

        var commits = new[]
        {
            new CommitAnalysisInput("a", "one", "Dev", "dev", DateTimeOffset.UtcNow),
            new CommitAnalysisInput("b", "two", "Dev", "dev", DateTimeOffset.UtcNow)
        };

        var stats = _sut.Analyze(files, commits);

        stats.FileCount.Should().Be(6);
        stats.CommitCount.Should().Be(2);
        stats.Additions.Should().Be(85);
        stats.Deletions.Should().Be(16);
        stats.TotalChangedLines.Should().Be(101);
        stats.LanguagesUsed.Should().BeEquivalentTo(["C#", "JSON", "SQL"]);
        stats.LargestModifiedFile.Should().Be("src/Services/PatientService.cs");
        stats.LargestModifiedFileChanges.Should().Be(50);
        stats.TestFilesChanged.Should().Be(1);
        stats.SqlFilesChanged.Should().Be(1);
        stats.ConfigurationFilesChanged.Should().Be(1);
        stats.ControllerFilesChanged.Should().Be(1);
        stats.ServiceFilesChanged.Should().Be(1);
        stats.SqlModified.Should().BeTrue();
    }

    [Theory]
    [InlineData("tests/FooTests.cs", true)]
    [InlineData("src/Foo.cs", false)]
    [InlineData("web/app.spec.ts", true)]
    [InlineData("src/Controllers/HomeController.cs", false)]
    public void IsTestFile_DetectsConventions(string filename, bool expected)
    {
        StatisticsAnalyzer.IsTestFile(File(filename, CodeLanguage.CSharp, 1, 0))
            .Should().Be(expected);
    }

    private static ReviewFileChange File(
        string filename,
        CodeLanguage language,
        int additions,
        int deletions,
        bool isBinary = false) =>
        new(
            filename,
            Path.GetExtension(filename),
            language,
            FileChangeStatus.Modified,
            additions,
            deletions,
            additions + deletions,
            isBinary ? null : "patch",
            isBinary,
            !isBinary && language != CodeLanguage.Unknown);
}
