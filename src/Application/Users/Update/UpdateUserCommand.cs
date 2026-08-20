using Application.Abstractions.Messaging;

namespace Application.Users.Update;

public sealed record UpdateUserCommand(Guid UserId, string Login, string DisplayName, string? OrganizationName = null) : ICommand;
