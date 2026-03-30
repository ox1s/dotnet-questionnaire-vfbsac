using SharedKernel;

namespace Domain.College.Specialities;

public static class SpecialityErrors
{
    public static Error NotFound(Guid specialityId) => Error.NotFound(
        "Specialities.NotFound",
        $"The speciality with the Id = '{specialityId}' was not found");
}
