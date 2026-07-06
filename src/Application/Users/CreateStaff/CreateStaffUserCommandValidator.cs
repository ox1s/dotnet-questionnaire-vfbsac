using Domain.User;
using FluentValidation;

namespace Application.Users.CreateStaff;

public class CreateStaffUserCommandValidator
    : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(r => r == UserRole.Staff || r == UserRole.DeputyHead)
            .WithMessage("Недопустимая роль для сотрудника.");
    }
}
