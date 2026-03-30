using Application.Abstractions.Messaging;

namespace Application.Users.SignIn;

public sealed record LoginUserCommand(string Login, string Password) : ICommand<string>;
