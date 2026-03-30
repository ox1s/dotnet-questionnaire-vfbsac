using SharedKernel;

namespace Domain.College.Disciplines;

public static class DisciplineErrors
{
    public static Error NotFound(Guid disciplineId) => Error.NotFound(
        "Disciplines.NotFound",
        $"The discipline with the Id = '{disciplineId}' was not found");

    public static Error DepartmentDeleted(Guid departmentId) => Error.Conflict(
        "Disciplines.DepartmentDeleted",
        $"Нельзя восстановить дисциплину, пока кафедра '{departmentId}' удалена.");
}
