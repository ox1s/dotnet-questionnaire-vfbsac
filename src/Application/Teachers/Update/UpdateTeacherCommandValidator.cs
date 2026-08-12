using FluentValidation;

namespace Application.Teachers.Update;

internal sealed class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(Resources.ApplicationErrors.NotEmpty)
            .MaximumLength(255).WithMessage("ФИО слишком длинное (макс. 255 символов)");

        RuleFor(x => x.DepartmentIds)
            .NotNull().WithMessage(Resources.ApplicationErrors.NotEmpty);
    }
}
