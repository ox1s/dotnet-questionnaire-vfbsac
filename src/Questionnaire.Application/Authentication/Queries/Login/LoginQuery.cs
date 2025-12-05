using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Authentication.Common;

namespace Questionnaire.Application.Authentication.Queries.Login;

public sealed record LoginQuery(
    string Login,
    string Password) : IQuery<AuthenticationResponse>;