using CodeSage.Application.Features.GitHub.Queries;
using FluentValidation;

namespace CodeSage.Application.Features.GitHub.Validators;

public sealed class GetRepositoryQueryValidator : AbstractValidator<GetRepositoryQuery>
{
    public GetRepositoryQueryValidator()
    {
        RuleFor(query => query.Owner).NotEmpty().MaximumLength(100);
        RuleFor(query => query.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class ListPullRequestsQueryValidator : AbstractValidator<ListPullRequestsQuery>
{
    public ListPullRequestsQueryValidator()
    {
        RuleFor(query => query.Owner).NotEmpty().MaximumLength(100);
        RuleFor(query => query.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class GetPullRequestQueryValidator : AbstractValidator<GetPullRequestQuery>
{
    public GetPullRequestQueryValidator()
    {
        RuleFor(query => query.Owner).NotEmpty().MaximumLength(100);
        RuleFor(query => query.Name).NotEmpty().MaximumLength(100);
        RuleFor(query => query.Number).GreaterThan(0);
    }
}
