using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Authentication.Common;

public static class AuthenticationErrors
{
    public static Error DuplicateLogin => Error.Conflict(
        "Auth.DuplicateLogin",
        "User with this login already exists.");

    public static Error InvalidCredentials => Error.Validation(
        "Auth.InvalidCredentials",
        "Invalid login or password.");
}