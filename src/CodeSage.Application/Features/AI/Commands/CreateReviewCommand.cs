using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.Analysis.Models;
using FluentValidation;
using MediatR;

namespace CodeSage.Application.Features.AI.Commands;

/// <summary>
/// Stateless AI review — returns a report without persistence.
/// </summary>
public sealed record CreateReviewCommand(ReviewContext Context) : IRequest<ReviewReport>;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(command => command.Context).NotNull();

        When(command => command.Context is not null, () =>
        {
            RuleFor(command => command.Context!.Repository).NotNull();
            When(command => command.Context!.Repository is not null, () =>
            {
                RuleFor(command => command.Context!.Repository.FullName)
                    .NotEmpty()
                    .MaximumLength(300)
                    .WithMessage("Repository.FullName is required (owner/name).");
                RuleFor(command => command.Context!.Repository.Name)
                    .NotEmpty()
                    .MaximumLength(200);
            });

            RuleFor(command => command.Context!.PullRequest).NotNull();
            When(command => command.Context!.PullRequest is not null, () =>
            {
                RuleFor(command => command.Context!.PullRequest.Number).GreaterThan(0);
                RuleFor(command => command.Context!.PullRequest.Title).NotEmpty().MaximumLength(500);
                RuleFor(command => command.Context!.PullRequest.BaseRef).NotEmpty();
                RuleFor(command => command.Context!.PullRequest.HeadRef).NotEmpty();
            });

            RuleFor(command => command.Context!.Author).NotNull();
            When(command => command.Context!.Author is not null, () =>
            {
                RuleFor(command => command.Context!.Author.Login).NotEmpty().MaximumLength(200);
            });

            RuleFor(command => command.Context!.Commits).NotNull();
            RuleFor(command => command.Context!.ChangedFiles).NotNull();
            RuleFor(command => command.Context!.Statistics).NotNull();
            RuleFor(command => command.Context!.LanguageBreakdown).NotNull();
            RuleFor(command => command.Context!.Summary).NotEmpty().MaximumLength(20_000);
        });
    }
}

public sealed class CreateReviewCommandHandler(IAIReviewService reviewService)
    : IRequestHandler<CreateReviewCommand, ReviewReport>
{
    public Task<ReviewReport> Handle(CreateReviewCommand request, CancellationToken cancellationToken) =>
        reviewService.ReviewAsync(request.Context, cancellationToken);
}
