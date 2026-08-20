using FluentValidation;

namespace Application.Forms.Create;

internal sealed class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.TargetRole)
            .IsInEnum()
            .When(x => x.TargetRole.HasValue);

        RuleForEach(x => x.Questions)
            .SetValidator(new QuestionRequestValidator());
    }
}
