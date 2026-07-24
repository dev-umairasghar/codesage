using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;

namespace CodeSage.UnitTests.Analysis;

public sealed class FileAnalyzerTests
{
    private readonly FileAnalyzer _sut = new(new LanguageAnalyzer());

    [Theory]
    [InlineData("added", FileChangeStatus.New)]
    [InlineData("modified", FileChangeStatus.Modified)]
    [InlineData("removed", FileChangeStatus.Deleted)]
    [InlineData("renamed", FileChangeStatus.Renamed)]
    [InlineData("weird", FileChangeStatus.Unknown)]
    public void Analyze_MapsStatus(string rawStatus, FileChangeStatus expected)
    {
        var result = _sut.Analyze(new ChangedFileAnalysisInput(
            "src/A.cs",
            rawStatus,
            1,
            0,
            1,
            "@@"));

        result.Status.Should().Be(expected);
        result.Language.Should().Be(CodeLanguage.CSharp);
        result.IsSupported.Should().BeTrue();
        result.Extension.Should().Be(".cs");
    }

    [Fact]
    public void Analyze_StripsPatch_ForBinaryFiles()
    {
        var result = _sut.Analyze(new ChangedFileAnalysisInput(
            "assets/logo.png",
            "added",
            0,
            0,
            0,
            "should-not-leak"));

        result.IsBinary.Should().BeTrue();
        result.IsSupported.Should().BeFalse();
        result.Patch.Should().BeNull();
        result.Language.Should().Be(CodeLanguage.Binary);
    }

    [Fact]
    public void Analyze_ComputesTotalChanges_WhenChangesMissing()
    {
        var result = _sut.Analyze(new ChangedFileAnalysisInput(
            "a.ts",
            "modified",
            Additions: 5,
            Deletions: 2,
            Changes: 0,
            Patch: "diff"));

        result.TotalChanges.Should().Be(7);
    }
}
