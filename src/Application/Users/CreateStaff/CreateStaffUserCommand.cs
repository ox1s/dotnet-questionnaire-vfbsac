using Application.Abstractions.Messaging;

namespace Application.Users.CreateStaff;

public sealed record CreateStaffUserCommand(
    string Login,
    string FullName,
    string Password,
    Guid? DepartmentId
) : ICommand<Guid>;
