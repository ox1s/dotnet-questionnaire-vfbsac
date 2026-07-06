using SharedKernel;

namespace Domain.College.Disciplines;

public static class DisciplineErrors
{
    public static Error NotFound(Guid disciplineId) => Error.NotFound(
        "Disciplines.NotFound",
        $"{Resources.DomainErrors.Disciplines_NotFound}, Id = '{disciplineId}'");

    public static Error DepartmentDeleted(Guid departmentId) => Error.Conflict(
        "Disciplines.DepartmentDeleted",
        $"{Resources.DomainErrors.Disciplines_DepartmentDeleted}, Id = '{departmentId}'");

    public static Error Duplicate(string name) => Error.Conflict(
        "Disciplines.DuplicateName",
        $"{Resources.DomainErrors.Disciplines_Duplicate}, Name = '{name}'");
}
