using CodeSage.Application.Features.Analysis.Models;
using CodeSage.Application.Features.GitHub.Dtos;

namespace CodeSage.Application.Features.Analysis.Mapping;

/// <summary>
/// Maps Application GitHub DTOs into provider-agnostic analysis input.
/// This is the last place GitHub-shaped data is allowed before ReviewContext.
/// </summary>
public static class PullRequestAnalysisInputMapper
{
    public static PullRequestAnalysisInput FromGitHub(
        string repositoryName,
        string repositoryFullName,
        string? repositoryDefaultBranch,
        PullRequestDetailsDto pullRequest)
    {
        ArgumentNullException.ThrowIfNull(pullRequest);

        return new PullRequestAnalysisInput(
            RepositoryName: repositoryName,
            RepositoryFullName: repositoryFullName,
            RepositoryDefaultBranch: repositoryDefaultBranch,
            PullRequestNumber: pullRequest.Number,
            Title: pullRequest.Title,
            Description: pullRequest.Description,
            State: pullRequest.State,
            IsDraft: pullRequest.Draft,
            AuthorLogin: pullRequest.AuthorLogin,
            AuthorDisplayName: null,
            AuthorAvatarUrl: pullRequest.AuthorAvatarUrl,
            BaseRef: pullRequest.BaseRef,
            HeadRef: pullRequest.HeadRef,
            CreatedAt: pullRequest.CreatedAt,
            UpdatedAt: pullRequest.UpdatedAt,
            Commits: pullRequest.Commits
                .Select(commit => new CommitAnalysisInput(
                    commit.Sha,
                    commit.Message,
                    commit.AuthorName,
                    commit.AuthorLogin,
                    commit.CommittedAt))
                .ToList(),
            ChangedFiles: pullRequest.ChangedFiles
                .Select(file => new ChangedFileAnalysisInput(
                    file.Filename,
                    file.Status,
                    file.Additions,
                    file.Deletions,
                    file.Changes,
                    file.Patch))
                .ToList());
    }
}
