namespace Application.Submissions.GetList;

public sealed record SubmissionResponse
{
    public Guid Id { get; init; }
    public Guid FormId { get; init; }
    public Guid UserId { get; init; }
    public DateTime SubmittedAt { get; init; }
    public SubmissionContextResponse Context { get; init; } = null!;
    public List<AnswerResponse> Answers { get; init; } = [];
}

public sealed record SubmissionContextResponse
{
    public Guid? DisciplineId { get; init; }
    public Guid? TeacherId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? SpecialityId { get; init; }
    public Guid? SpecializationId { get; init; }
    public string? OrganizationName { get; init; }
}

public sealed record AnswerResponse
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string? Value { get; init; }
    public decimal? NumericValue { get; init; }
    public decimal? Weight { get; init; }
}
