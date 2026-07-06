using FluentValidation;

namespace Application.Forms.Create;

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
