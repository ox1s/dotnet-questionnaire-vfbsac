using SharedKernel;

namespace Domain.College.Specializations;

public static class SpecializationErrors
{
    public static Error NotFound(Guid specializationId) => Error.NotFound(
        "Specializations.NotFound",
        $"The specialization with the Id = '{specializationId}' was not found");
}
