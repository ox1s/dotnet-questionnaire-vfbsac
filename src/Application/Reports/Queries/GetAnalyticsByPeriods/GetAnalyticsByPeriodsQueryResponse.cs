using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByPeriods;

public sealed record GetAnalyticsByPeriodsQueryResponse(
    string Label,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    List<QuestionStatistics> QuestionStatistics,
    OverallSatisfaction Overall,
    int SubmissionCount);
