using FluentValidation;

namespace Application.Disciplines.Create;

internal sealed class CreateDisciplineCommandValidator : AbstractValidator<CreateDisciplineCommand>
{
    public CreateDisciplineCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название дисциплины обязательно")
            .MaximumLength(255);

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Укажите кафедру");
    }
}
