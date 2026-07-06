namespace Application.Reports.Queries.GetAnalyticsByPeriod;

public sealed record GetAnalyticsByPeriodQueryResponse(
    Guid QuestionId,
    string QuestionText,
    decimal Median,
    decimal Mean,
    decimal Mode,
    decimal StandardDeviation,
    int ResponseCount);
