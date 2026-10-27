using FluentValidation;

namespace Application.Users.CreateTeacher;

internal sealed class CreateTeacherUserCommandValidator
    : AbstractValidator<CreateTeacherUserCommand>
{
    public CreateTeacherUserCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty()
            .WithMessage("Преподаватель обязателен.");

        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Логин обязателен.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен.")
            .MinimumLength(12)
            .WithMessage("Пароль должен содержать не менее 12 символов.");
    }
}
