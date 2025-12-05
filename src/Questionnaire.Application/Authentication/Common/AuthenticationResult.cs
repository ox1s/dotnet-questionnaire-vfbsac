using Questionnaire.Domain.Users;

namespace Questionnaire.Application.Authentication.Common;

public record AuthenticationResult(
    User User,
    string Token);