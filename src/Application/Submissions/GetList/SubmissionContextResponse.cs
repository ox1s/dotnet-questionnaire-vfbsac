namespace Application.Submissions.GetList;

public sealed record SubmissionContextResponse
{
    public Guid? DisciplineId { get; init; }
    public Guid? TeacherId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? SpecialityId { get; init; }
    public Guid? SpecializationId { get; init; }
    public string? OrganizationName { get; init; }
}
