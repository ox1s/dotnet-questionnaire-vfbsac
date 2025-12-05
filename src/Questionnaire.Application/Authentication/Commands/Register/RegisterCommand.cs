using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Authentication.Common;

namespace Questionnaire.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(
    string Login,
    string Password,
    string Role) : ICommand<AuthenticationResponse>;