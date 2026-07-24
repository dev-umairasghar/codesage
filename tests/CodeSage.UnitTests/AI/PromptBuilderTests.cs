using CodeSage.Application.Features.AI;
using CodeSage.Application.Features.AI.Services;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CodeSage.UnitTests.AI;

public sealed class PromptBuilderTests
{
    [Fact]
    public void Build_SeparatesSystemDeveloperAndUserPrompts()
    {
        var sut = new PromptBuilder(Options.Create(new AiReviewOptions()));
        var prompt = sut.Build(CreateContext());

        prompt.SystemPrompt.Should().Contain("CodeSage");
        prompt.DeveloperPrompt.Should().Contain("reviewSummary");
        prompt.DeveloperPrompt.Should().Contain("overallRisk");
        prompt.UserPrompt.Should().Contain("Healthcare.API");
        prompt.UserPrompt.Should().Contain("PatientService.cs");
        prompt.UserPrompt.Should().Contain("## Deterministic Summary");
        prompt.UserPrompt.Should().Contain("patch content");
    }

    [Fact]
    public void Build_TruncatesLargePatches_AndSkipsBinary()
    {
        var sut = new PromptBuilder(Options.Create(new AiReviewOptions
        {
            MaxPatchCharactersPerFile = 20,
            MaxFilesWithPatches = 10
        }));

        var context = CreateContext() with
        {
            ChangedFiles =
            [
                new ReviewFileChange(
                    "src/A.cs",
                    ".cs",
                    CodeLanguage.CSharp,
                    FileChangeStatus.Modified,
                    1,
                    0,
                    1,
                    new string('x', 100),
                    false,
                    true),
                new ReviewFileChange(
                    "logo.png",
                    ".png",
                    CodeLanguage.Binary,
                    FileChangeStatus.New,
                    0,
                    0,
                    0,
                    "secret-binary",
                    true,
                    false)
            ]
        };

        var prompt = sut.Build(context);

        prompt.UserPrompt.Should().Contain("[truncated");
        prompt.UserPrompt.Should().NotContain("secret-binary");
    }

    private static ReviewContext CreateContext() =>
        new(
            new ReviewRepositoryInfo("Healthcare.API", "acme/Healthcare.API", "main"),
            new ReviewPullRequestInfo(1, "Title", "Body", "open", false, "main", "feature", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new ReviewAuthorInfo("dev", "Dev", null),
            [new ReviewCommitInfo("abcdef0", "msg", "Dev", "dev", DateTimeOffset.UtcNow)],
            [
                new ReviewFileChange(
                    "src/Services/PatientService.cs",
                    ".cs",
                    CodeLanguage.CSharp,
                    FileChangeStatus.Modified,
                    10,
                    2,
                    12,
                    "patch content",
                    false,
                    true)
            ],
            new ReviewStatistics(1, 1, 10, 2, 12, ["C#"], "src/Services/PatientService.cs", 12, 0, 0, 0, 0, 1, false),
            new Dictionary<string, int> { ["C#"] = 1 },
            "Repository:\nHealthcare.API");
}
