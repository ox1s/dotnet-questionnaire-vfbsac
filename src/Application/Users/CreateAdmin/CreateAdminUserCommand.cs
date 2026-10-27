using Application.Abstractions.Messaging;

namespace Application.Users.CreateAdmin;

public sealed record CreateAdminUserCommand(
    string Login,
    string Password
) : ICommand<Guid>;
