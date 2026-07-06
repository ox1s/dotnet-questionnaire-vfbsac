using SharedKernel;

namespace Domain.College.Departments;

public static class DepartmentErrors
{
    public static Error NotFound(Guid departmentId) => Error.NotFound(
        "Departments.NotFound",
        $"{Resources.DomainErrors.Departments_NotFound}, Id = '{departmentId}'");
    public static Error Duplicate => Error.Conflict(
        "Departments.Duplicate",
        $"{Resources.DomainErrors.Departments_Duplicate}");

    public static Error HasTeachers() => Error.Conflict(
        "Departments.HasTeachers",
        $"{Resources.DomainErrors.Departments_HasTeachers}");

    public static Error HasDisciplines() => Error.Conflict(
        "Departments.HasDisciplines",
        $"{Resources.DomainErrors.Departments_HasDisciplines}");

    public static Error HasUsers() => Error.Conflict(
        "Departments.HasUsers",
        $"{Resources.DomainErrors.Departments_HasUsers}");
}
