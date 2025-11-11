using ErrorOr;
using MediatR;
using Questionnaire.Application.Authentication.Common;

namespace Questionnaire.Application.Authentication.Queries.Login;

public record LoginQuery(
    string Login,
    string Password) : IRequest<ErrorOr<AuthenticationResult>>;