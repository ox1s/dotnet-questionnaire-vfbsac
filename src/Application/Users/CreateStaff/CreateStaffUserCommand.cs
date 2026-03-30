using Application.Abstractions.Messaging;
using Domain.User;

namespace Application.Users.CreateStaff;

public sealed record CreateStaffUserCommand(
    string Login,
    string FullName,
    string Password,
    Guid? DepartmentId,
    UserRole Role
) : ICommand<Guid>;
