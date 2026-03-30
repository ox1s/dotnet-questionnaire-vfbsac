namespace Application.Submissions.GetList;

public sealed record AnswerResponse
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string? Value { get; init; }
    public decimal? NumericValue { get; init; }
    public decimal? Weight { get; init; }
}
