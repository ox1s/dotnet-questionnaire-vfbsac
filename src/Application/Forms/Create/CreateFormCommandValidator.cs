using FluentValidation;

namespace Application.Forms.Create;

internal sealed class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleForEach(x => x.Questions)
            .SetValidator(new QuestionRequestValidator());
    }
}
