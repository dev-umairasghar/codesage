using CodeSage.Application.Features.Analysis.Queries;
using FluentValidation;

namespace CodeSage.Application.Features.Analysis.Validators;

public sealed class AnalyzePullRequestQueryValidator : AbstractValidator<AnalyzePullRequestQuery>
{
    public AnalyzePullRequestQueryValidator()
    {
        RuleFor(query => query.Owner).NotEmpty().MaximumLength(100);
        RuleFor(query => query.Name).NotEmpty().MaximumLength(100);
        RuleFor(query => query.Number).GreaterThan(0);
    }
}
