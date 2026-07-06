namespace Application.Reports.Queries.Shared;

public sealed record QuestionStatistics(
    Guid QuestionId,
    string QuestionText,
    decimal Median,
    decimal Mean,
    decimal Mode,
    decimal StandardDeviation,
    int ResponseCount);
