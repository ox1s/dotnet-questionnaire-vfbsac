using ErrorOr;

namespace Questionnaire.Application.Authentication.Common;

public static class AuthenticationErrors
{
    public static Error DuplicateLogin => Error.Conflict(
        code: "Auth.DuplicateLogin",
        description: "User with this login already exists.");

    public static Error InvalidCredentials => Error.Validation(
        code: "Auth.InvalidCredentials",
        description: "Invalid login or password.");
}