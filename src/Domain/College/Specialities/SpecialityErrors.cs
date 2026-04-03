using SharedKernel;

namespace Domain.College.Specialities;

public static class SpecialityErrors
{
    public static Error NotFound(Guid specialityId) => Error.NotFound(
        "Specialities.NotFound",
        $"{Resources.DomainErrors.Specialities_NotFound}, Id = '{specialityId}'");

    public static Error HasSpecializations() => Error.Conflict(
        "Specialities.HasSpecializations",
        $"{Resources.DomainErrors.Specialities_HasSpecializations}");

    public static Error Duplicate(string name) => Error.Conflict(
        "Specialities.Duplicate",
        $"{Resources.DomainErrors.Specialities_Duplicate}, Name = '{name}'");
}
