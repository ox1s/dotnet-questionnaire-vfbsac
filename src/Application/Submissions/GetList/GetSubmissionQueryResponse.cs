namespace Application.Submissions.GetList;

public sealed record GetSubmissionQueryResponse
{
    public Guid Id { get; init; }
    public Guid FormId { get; init; }
    public Guid UserId { get; init; }
    public DateTime SubmittedAt { get; init; }
    public SubmissionContextResponse Context { get; init; } = null!;
    public List<AnswerResponse> Answers { get; init; } = [];
}
