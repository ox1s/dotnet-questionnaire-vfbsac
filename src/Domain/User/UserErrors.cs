using SharedKernel;

namespace Domain.UserAggregate;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error NotFoundByLogin(string login) => Error.NotFound(
        "Users.NotFoundByLogin",
        $"The user with the Login = '{login}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static Error InvalidResetToken() => Error.Validation(
        "Users.InvalidResetToken",
        "Invalid or expired password reset token.");

    public static Error ExpiredResetToken() => Error.Validation(
        "Users.ExpiredResetToken",
        "The password reset token has expired.");

    public static Error Conflict(string code, string description) =>
        Error.Conflict(code, description);
}
