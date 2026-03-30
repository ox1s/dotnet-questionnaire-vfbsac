using SharedKernel;

namespace Domain.College.Teachers;

public static class TeacherErrors
{
    public static Error NotFound(Guid teacherId) => Error.NotFound(
        "Teachers.NotFound",
        $"The teacher with the Id = '{teacherId}' was not found");
}
