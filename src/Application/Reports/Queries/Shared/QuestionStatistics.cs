namespace Application.Reports.Queries.Shared;

public sealed record QuestionStatistics(
    Guid QuestionId,
    string QuestionText,
    decimal SatisfactionPercentage,
    decimal AverageScore,
    decimal StandardDeviation,
    SatisfactionRating Rating,
    int ResponseCount);
