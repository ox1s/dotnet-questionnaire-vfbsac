using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(int userId) => Error.NotFound(
        "User.NotFound",
        $"The user with Id = '{userId}' was not found.");

    public static Error NotFoundByLogin(string login) => Error.NotFound(
        "User.NotFoundByLogin",
        $"The user with login '{login}' was not found.");

    public static Error AlreadyExists(string login) => Error.Conflict(
        "User.AlreadyExists",
        $"The user with login '{login}' already exists.");
}
