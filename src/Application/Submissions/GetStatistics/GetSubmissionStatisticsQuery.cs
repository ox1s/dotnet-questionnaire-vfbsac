using Application.Abstractions.Messaging;

namespace Application.Submissions.GetStatistics;

public sealed record GetSubmissionStatisticsQuery(
    Guid FormId,
    Guid? DisciplineId = null,
    Guid? TeacherId = null,
    Guid? DepartmentId = null,
    Guid? SpecialityId = null,
    Guid? SpecializationId = null,
    string? OrganizationName = null)
    : IQuery<SubmissionStatisticsResponse>;
