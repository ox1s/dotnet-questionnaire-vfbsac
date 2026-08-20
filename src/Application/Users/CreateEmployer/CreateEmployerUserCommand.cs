using Application.Abstractions.Messaging;

namespace Application.Users.CreateEmployer;

public sealed record CreateEmployerUserCommand(
    string Login,
    string DisplayName,
    string OrganizationName,
    string Password
) : ICommand<Guid>;
