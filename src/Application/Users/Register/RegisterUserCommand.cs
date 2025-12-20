using Application.Abstractions.Messaging;

namespace Application.Users.Register;

public sealed record RegisterUserCommand(string Login, string DisplayName, string Password)
    : ICommand<Guid>;
