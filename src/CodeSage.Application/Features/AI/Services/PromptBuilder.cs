using System.Text;
using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.AI.Prompts;
using CodeSage.Application.Features.Analysis.Models;
using Microsoft.Extensions.Options;

namespace CodeSage.Application.Features.AI.Services;

/// <summary>
/// Constructs System / Developer / User prompts from <see cref="ReviewContext"/>.
/// Patch inclusion is budgeted so prompts stay within model context limits.
/// </summary>
public sealed class PromptBuilder(IOptions<AiReviewOptions> options) : IPromptBuilder
{
    /// <inheritdoc />
    public AiPrompt Build(ReviewContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = options.Value;
        var userPrompt = BuildUserPrompt(context, settings);

        return new AiPrompt(
            SystemPrompt: ReviewPromptTemplates.SystemPrompt.Trim(),
            DeveloperPrompt: ReviewPromptTemplates.DeveloperPrompt.Trim(),
            UserPrompt: userPrompt);
    }

    private static string BuildUserPrompt(ReviewContext context, AiReviewOptions settings)
    {
        var builder = new StringBuilder();
        builder.AppendLine(ReviewPromptTemplates.UserPromptHeader.Trim());
        builder.AppendLine();
        builder.AppendLine("## Deterministic Summary");
        builder.AppendLine(context.Summary);
        builder.AppendLine();
        builder.AppendLine("## Repository");
        builder.AppendLine($"Name: {context.Repository.Name}");
        builder.AppendLine($"FullName: {context.Repository.FullName}");
        builder.AppendLine($"DefaultBranch: {context.Repository.DefaultBranch}");
        builder.AppendLine();
        builder.AppendLine("## Pull Request");
        builder.AppendLine($"Number: {context.PullRequest.Number}");
        builder.AppendLine($"Title: {context.PullRequest.Title}");
        builder.AppendLine($"State: {context.PullRequest.State}");
        builder.AppendLine($"Draft: {context.PullRequest.IsDraft}");
        builder.AppendLine($"Base: {context.PullRequest.BaseRef}");
        builder.AppendLine($"Head: {context.PullRequest.HeadRef}");
        builder.AppendLine("Description:");
        builder.AppendLine(context.PullRequest.Description ?? "(none)");
        builder.AppendLine();
        builder.AppendLine("## Author");
        builder.AppendLine($"Login: {context.Author.Login}");
        builder.AppendLine($"DisplayName: {context.Author.DisplayName ?? "(none)"}");
        builder.AppendLine();
        builder.AppendLine("## Statistics");
        builder.AppendLine($"Files: {context.Statistics.FileCount}");
        builder.AppendLine($"Commits: {context.Statistics.CommitCount}");
        builder.AppendLine($"Additions: {context.Statistics.Additions}");
        builder.AppendLine($"Deletions: {context.Statistics.Deletions}");
        builder.AppendLine($"TotalChangedLines: {context.Statistics.TotalChangedLines}");
        builder.AppendLine($"Languages: {string.Join(", ", context.Statistics.LanguagesUsed)}");
        builder.AppendLine($"LargestFile: {context.Statistics.LargestModifiedFile ?? "(none)"}");
        builder.AppendLine($"TestsChanged: {context.Statistics.TestFilesChanged}");
        builder.AppendLine($"SqlModified: {context.Statistics.SqlModified}");
        builder.AppendLine();
        builder.AppendLine("## Language Breakdown");
        foreach (var pair in context.LanguageBreakdown)
        {
            builder.AppendLine($"- {pair.Key}: {pair.Value}");
        }

        builder.AppendLine();
        builder.AppendLine("## Commits");
        foreach (var commit in context.Commits)
        {
            builder.AppendLine($"- {commit.Sha[..Math.Min(7, commit.Sha.Length)]}: {commit.Message} ({commit.AuthorLogin ?? commit.AuthorName})");
        }

        builder.AppendLine();
        builder.AppendLine("## Changed Files");
        var patchBudgetFiles = 0;
        foreach (var file in context.ChangedFiles)
        {
            builder.AppendLine(
                $"- {file.Filename} [{file.Status}] lang={file.Language} +{file.Additions}/-{file.Deletions} supported={file.IsSupported} binary={file.IsBinary}");

            if (file.IsBinary || !file.IsSupported || string.IsNullOrWhiteSpace(file.Patch))
            {
                continue;
            }

            if (patchBudgetFiles >= settings.MaxFilesWithPatches)
            {
                continue;
            }

            patchBudgetFiles++;
            builder.AppendLine("Patch:");
            builder.AppendLine(Truncate(file.Patch, settings.MaxPatchCharactersPerFile));
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + $"{Environment.NewLine}...[truncated {value.Length - maxLength} chars]";
    }
}
