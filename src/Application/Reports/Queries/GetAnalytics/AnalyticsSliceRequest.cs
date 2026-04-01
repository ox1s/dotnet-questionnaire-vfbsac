namespace Application.Reports.Queries.GetAnalytics;

public sealed record AnalyticsSliceRequest(
    string Label,
    DateTime DateFrom,
    DateTime DateTo,
    Guid? DisciplineId = null,
    Guid? TeacherId = null,
    Guid? DepartmentId = null,
    Guid? SpecialityId = null,
    Guid? SpecializationId = null,
    string? OrganizationName = null)
{
    public AnalyticsFilterSet ToFilterSet()
    {
        return new AnalyticsFilterSet(
            DisciplineId,
            TeacherId,
            DepartmentId,
            SpecialityId,
            SpecializationId,
            OrganizationName);
    }
}
