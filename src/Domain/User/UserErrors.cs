using SharedKernel;

namespace Domain.User;

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
    
    public static Error UserExist() => Error.Conflict(
        "Users.UserExists",
        "User already exists.");
    public static Error GroupExists(string login) => Error.Conflict(
        "Users.GroupExists", 
        $"Group with {login} already exists");

    public static Error GroupNameInvalid() => Error.Failure(
        "Users.GroupNameInvalid",
        "Group name is invalid (should be less than 5 characters).");
}
