using FluentValidation;

namespace Questionnaire.Application.Forms.Commands.Create;

internal sealed class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Form name is required.")
            .MaximumLength(200)
            .WithMessage("Form name must not exceed 200 characters.");
    }
}