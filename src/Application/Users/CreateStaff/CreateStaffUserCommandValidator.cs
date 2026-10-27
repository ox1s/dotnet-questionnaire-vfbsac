using FluentValidation;

namespace Application.Users.CreateStaff;

public class CreateStaffUserCommandValidator
    : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Логин обязателен.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("ФИО сотрудника обязательно.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен.")
            .MinimumLength(12)
            .WithMessage("Пароль должен содержать не менее 12 символов.");
    }
}
