using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByPeriod;

public sealed record GetAnalyticsByPeriodQueryResponse(
    Guid QuestionId,
    string QuestionText,
    decimal SatisfactionPercentage,
    decimal AverageScore,
    decimal StandardDeviation,
    SatisfactionRating Rating,
    int ResponseCount);
