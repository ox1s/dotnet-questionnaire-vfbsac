using Application.Abstractions.Messaging;
using Domain.UserAggregate;

namespace Application.Users.CreateStaff;

public sealed record CreateStaffUserCommand(
    string Login,
    string FullName,
    string Password,
    Guid? DepartmentId,
    UserRole Role // Staff или DeputyHead
) : ICommand<Guid>;
