using CodeSage.Application.Features.Analysis.Mapping;
using CodeSage.Application.Features.GitHub.Dtos;
using FluentAssertions;

namespace CodeSage.UnitTests.Analysis;

public sealed class PullRequestAnalysisInputMapperTests
{
    [Fact]
    public void FromGitHub_MapsDtoFields_WithoutLeakingVendorNamesIntoModel()
    {
        var dto = new PullRequestDetailsDto(
            Number: 7,
            Title: "Title",
            Description: "Desc",
            State: "open",
            Draft: true,
            AuthorLogin: "dev",
            AuthorAvatarUrl: "https://example.com/a.png",
            CreatedAt: DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
            UpdatedAt: DateTimeOffset.Parse("2024-01-02T00:00:00Z"),
            HtmlUrl: "https://github.com/org/repo/pull/7",
            BaseRef: "main",
            HeadRef: "feature",
            ChangedFiles:
            [
                new ChangedFileDto("a.cs", "added", 1, 0, 1, "patch")
            ],
            Commits:
            [
                new CommitSummaryDto("sha", "msg", "Dev", "dev", DateTimeOffset.Parse("2024-01-01T01:00:00Z"))
            ],
            Comments: []);

        var input = PullRequestAnalysisInputMapper.FromGitHub("repo", "org/repo", "main", dto);

        input.RepositoryName.Should().Be("repo");
        input.PullRequestNumber.Should().Be(7);
        input.IsDraft.Should().BeTrue();
        input.ChangedFiles.Should().ContainSingle(file => file.Filename == "a.cs" && file.RawStatus == "added");
        input.Commits.Should().ContainSingle(commit => commit.Sha == "sha");

        // ReviewContext path never needs HtmlUrl — mapper drops vendor-only fields by design.
        input.GetType().GetProperties().Select(property => property.Name)
            .Should().NotContain("HtmlUrl");
    }
}
