using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Login).NotEmpty();
        RuleFor(c => c.DisplayName).NotEmpty();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
