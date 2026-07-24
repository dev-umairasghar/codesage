using CodeSage.Application.Features.Analysis.Analyzers;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;

namespace CodeSage.UnitTests.Analysis;

public sealed class LanguageAnalyzerTests
{
    private readonly LanguageAnalyzer _sut = new();

    [Theory]
    [InlineData("src/Foo.cs", CodeLanguage.CSharp)]
    [InlineData("db/script.SQL", CodeLanguage.Sql)]
    [InlineData("app.js", CodeLanguage.JavaScript)]
    [InlineData("app.mjs", CodeLanguage.JavaScript)]
    [InlineData("component.tsx", CodeLanguage.TypeScript)]
    [InlineData("config.json", CodeLanguage.Json)]
    [InlineData("compose.yaml", CodeLanguage.Yaml)]
    [InlineData("compose.yml", CodeLanguage.Yaml)]
    [InlineData("README.md", CodeLanguage.Markdown)]
    [InlineData("CodeSage.csproj", CodeLanguage.Xml)]
    [InlineData("web.config", CodeLanguage.Xml)]
    [InlineData("unknown.xyz", CodeLanguage.Unknown)]
    public void DetectLanguage_MapsKnownExtensions(string filename, CodeLanguage expected)
    {
        _sut.DetectLanguage(filename).Should().Be(expected);
    }

    [Theory]
    [InlineData("logo.png")]
    [InlineData("lib/CodeSage.dll")]
    [InlineData("archive.zip")]
    public void IsBinary_DetectsCommonBinaryExtensions(string filename)
    {
        _sut.IsBinary(filename, patch: null).Should().BeTrue();
        _sut.DetectLanguage(filename).Should().Be(CodeLanguage.Binary);
    }

    [Fact]
    public void IsBinary_DetectsGitBinaryPatchMarker()
    {
        _sut.IsBinary("mystery.bin", "GIT binary patch\nliteral 10").Should().BeTrue();
    }

    [Fact]
    public void GetExtension_NormalizesSeparators()
    {
        _sut.GetExtension(@"src\Services\Foo.cs").Should().Be(".cs");
    }
}
