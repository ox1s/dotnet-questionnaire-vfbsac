using Application.Abstractions.Messaging;

namespace Application.Users.CreateGroup;

public sealed record CreateGroupUserCommand(string GroupName, string Password) : ICommand<Guid>;
