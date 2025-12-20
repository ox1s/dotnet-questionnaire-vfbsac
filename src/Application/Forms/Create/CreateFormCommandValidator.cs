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

internal sealed class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0);
    }
}
