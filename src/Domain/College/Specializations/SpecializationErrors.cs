using SharedKernel;

namespace Domain.College.Specializations;

public static class SpecializationErrors
{
    public static Error NotFound(Guid specializationId) => Error.NotFound(
        "Specializations.NotFound",
        $"{Resources.DomainErrors.Specializations_NotFound}, Id = '{specializationId}'");

    public static Error SpecialityDeleted(Guid specialityId) => Error.Conflict(
        "Specializations.SpecialityDeleted",
        $"{Resources.DomainErrors.Specializations_SpecialityDeleted}, Id = '{specialityId}'");

    public static Error Duplicate(string name) => Error.Conflict(
        "Specializations.Duplicate",
        $"{Resources.DomainErrors.Specializations_Duplicate}, Name = '{name}'");
}
