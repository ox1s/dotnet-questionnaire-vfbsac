using SharedKernel;

namespace Domain.College.Disciplines;

public static class DisciplineErrors
{
    public static Error NotFound(Guid disciplineId) => Error.NotFound(
        "Disciplines.NotFound",
        $"The discipline with the Id = '{disciplineId}' was not found");
}
