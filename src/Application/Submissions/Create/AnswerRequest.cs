namespace Application.Submissions.Create;

public sealed record AnswerRequest(
    Guid QuestionId,
    string? Value = null,
    decimal? NumericValue = null,
    decimal? Weight = null);
