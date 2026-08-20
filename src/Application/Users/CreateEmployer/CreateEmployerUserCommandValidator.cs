using FluentValidation;

namespace Application.Users.CreateEmployer;

public class CreateEmployerUserCommandValidator
    : AbstractValidator<CreateEmployerUserCommand>
{
    public CreateEmployerUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Логин обязателен.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Имя нанимателя обязательно.");

        RuleFor(x => x.OrganizationName)
            .NotEmpty()
            .WithMessage("Название организации обязательно.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен.")
            .MinimumLength(12)
            .WithMessage("Пароль должен содержать не менее 12 символов.");
    }
}
