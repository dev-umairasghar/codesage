using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.AI;
using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.AI.Services;
using CodeSage.Application.Features.Analysis.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace CodeSage.UnitTests.AI;

public sealed class AiReviewServiceTests
{
    [Fact]
    public async Task ReviewAsync_OrchestratesPromptProviderAndParser()
    {
        var provider = Substitute.For<IAIProvider>();
        provider.Name.Returns("MockAI");
        provider.CompleteAsync(Arg.Any<AiCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletionResult(
                """
                {"reviewSummary":"Looks good","overallRisk":"Low","positiveFindings":["Clean diff"],"issues":[],"recommendations":[],"missingTests":[],"securityConcerns":[],"performanceConcerns":[],"maintainability":[],"architectureConcerns":[]}
                """,
                "mock-model",
                10,
                20,
                30,
                TimeSpan.FromMilliseconds(12)));

        var sut = new AiReviewService(
            new PromptBuilder(Options.Create(new AiReviewOptions())),
            provider,
            new ReviewParser(),
            Options.Create(new AiReviewOptions { LogPrompts = false }),
            NullLogger<AiReviewService>.Instance);

        var report = await sut.ReviewAsync(CreateContext());

        report.Summary.Should().Be("Looks good");
        report.OverallRisk.Should().Be(ReviewRiskLevel.Low);
        report.Model.Should().Be("mock-model");
        await provider.Received(1).CompleteAsync(
            Arg.Is<AiCompletionRequest>(request =>
                request.Prompt.SystemPrompt.Length > 0
                && request.Prompt.DeveloperPrompt.Contains("reviewSummary")
                && request.Prompt.UserPrompt.Contains("Healthcare.API")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReviewAsync_WhenProviderFails_PropagatesAiException()
    {
        var provider = Substitute.For<IAIProvider>();
        provider.Name.Returns("MockAI");
        provider.CompleteAsync(Arg.Any<AiCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<AiCompletionResult>>(_ => throw new AiRateLimitException("slow down"));

        var sut = new AiReviewService(
            new PromptBuilder(Options.Create(new AiReviewOptions())),
            provider,
            new ReviewParser(),
            Options.Create(new AiReviewOptions()),
            NullLogger<AiReviewService>.Instance);

        var act = async () => await sut.ReviewAsync(CreateContext());

        await act.Should().ThrowAsync<AiRateLimitException>();
    }

    private static ReviewContext CreateContext() =>
        new(
            new ReviewRepositoryInfo("Healthcare.API", "acme/Healthcare.API", "main"),
            new ReviewPullRequestInfo(7, "Title", "Body", "open", false, "main", "feature", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new ReviewAuthorInfo("dev", null, null),
            [],
            [
                new ReviewFileChange("a.cs", ".cs", CodeLanguage.CSharp, FileChangeStatus.Modified, 1, 0, 1, "diff", false, true)
            ],
            new ReviewStatistics(1, 0, 1, 0, 1, ["C#"], "a.cs", 1, 0, 0, 0, 0, 0, false),
            new Dictionary<string, int> { ["C#"] = 1 },
            "Repository:\nHealthcare.API");
}
