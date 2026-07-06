using SharedKernel;

namespace Domain.College.Teachers;

public static class TeacherErrors
{
    public static Error NotFound(Guid teacherId) => Error.NotFound(
        "Teachers.NotFound",
        $"{Resources.DomainErrors.Teachers_NotFound}, Id = '{teacherId}'");

    public static Error HasUsers() => Error.Conflict(
        "Teachers.HasUsers",
        $"{Resources.DomainErrors.Teachers_HasUsers}");
}
