namespace Application.Reports.Queries.GetAnalytics;

public sealed record AnalyticsFilterSet(
    Guid? DisciplineId = null,
    Guid? TeacherId = null,
    Guid? DepartmentId = null,
    Guid? SpecialityId = null,
    Guid? SpecializationId = null,
    string? OrganizationName = null);
