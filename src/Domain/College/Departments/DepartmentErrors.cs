using SharedKernel;

namespace Domain.College.Departments;

public static class DepartmentErrors
{
    public static Error NotFound(Guid departmentId) => Error.NotFound(
        "Departments.NotFound",
        $"The department with the Id = '{departmentId}' was not found");
    public static Error Duplicate => Error.Conflict(
        "Departments.Duplicate",
        "A department with the same name already exists");

    public static Error HasTeachers() => Error.Conflict(
        "Departments.HasTeachers",
        "К кафедре привязаны преподаватели, поэтому сначала уберите их.");

    public static Error HasDisciplines() => Error.Conflict(
        "Departments.HasDisciplines",
        "К кафедре привязаны дисциплины, поэтому сначала уберите их.");

    public static Error HasUsers() => Error.Conflict(
        "Departments.HasUsers",
        "К кафедре привязаны пользователи, поэтому сначала уберите их.");
}
