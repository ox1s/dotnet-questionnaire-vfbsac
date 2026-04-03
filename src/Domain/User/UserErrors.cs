using SharedKernel;

namespace Domain.User;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"{Resources.DomainErrors.Users_NotFound}, Id = '{userId}'");

    public static Error NotFoundByLogin(string login) => Error.NotFound(
        "Users.NotFoundByLogin",
        $"{Resources.DomainErrors.Users_NotFoundByLogin}, Login = '{login}'");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        $"{Resources.DomainErrors.Users_Unauthorized}");

    public static Error InvalidResetToken() => Error.Validation(
        "Users.InvalidResetToken",
        $"{Resources.DomainErrors.Users_InvalidResetToken}");

    public static Error ExpiredResetToken() => Error.Validation(
        "Users.ExpiredResetToken",
        $"{Resources.DomainErrors.Users_ExpiredResetToken}");
    
    public static Error UserExist() => Error.Conflict(
        "Users.UserExists",
        $"{Resources.DomainErrors.Users_UserExist}");
    public static Error GroupExists(string login) => Error.Conflict(
        "Users.GroupExists", 
        $"{Resources.DomainErrors.Users_GroupExists}, Login = {login}");

    public static Error GroupNameInvalid() => Error.Failure(
        "Users.GroupNameInvalid",
        $"{Resources.DomainErrors.Users_GroupNameInvalid}");
}
