using ErrorOr;
using MediatR;
using Questionnaire.Application.Authentication.Common;

namespace Questionnaire.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string Login,
    string Password,
    string Role) : IRequest<ErrorOr<AuthenticationResult>>;