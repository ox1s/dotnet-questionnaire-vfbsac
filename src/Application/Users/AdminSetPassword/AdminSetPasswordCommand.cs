using Application.Abstractions.Messaging;

namespace Application.Users.AdminSetPassword;

public sealed record AdminSetPasswordCommand(Guid UserId, string NewPassword) : ICommand;
