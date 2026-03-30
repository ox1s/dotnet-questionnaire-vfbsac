using FluentValidation;

namespace Application.Teachers.Create;

internal sealed class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("ФИО преподавателя не может быть пустым")
            .MaximumLength(255).WithMessage("ФИО слишком длинное (макс. 255 символов)");
    }
}
