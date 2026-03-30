using SharedKernel;

namespace Domain.College.Speciality;

public static class SpecialityErrors
{
    public static Error NotFound(Guid specialityId) => Error.NotFound(
        "Specialities.NotFound",
        $"The speciality with the Id = '{specialityId}' was not found");
}
