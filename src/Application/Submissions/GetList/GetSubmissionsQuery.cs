using Application.Abstractions.Messaging;

namespace Application.Submissions.GetList;

public sealed record GetSubmissionsQuery(
    Guid? FormId = null,
    string? DeviceId = null,
    Guid? UserId = null,
    Guid? DisciplineId = null,
    Guid? TeacherId = null,
    Guid? DepartmentId = null,
    Guid? SpecialityId = null,
    Guid? SpecializationId = null,
    string? OrganizationName = null,
    DateTime? SubmittedFrom = null,
    DateTime? SubmittedTo = null)
    : IQuery<List<GetSubmissionsQueryResponse>>;
