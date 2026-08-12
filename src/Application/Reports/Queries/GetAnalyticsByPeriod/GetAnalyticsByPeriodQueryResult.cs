using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByPeriod;

/// <summary>
/// Container response for the period analytics query: the per-question breakdown plus the
/// formula (5)/(6) overall form/blank satisfaction and the distinct submission count, both
/// computed once for the whole form/period rather than duplicated onto every question row.
/// </summary>
public sealed record GetAnalyticsByPeriodQueryResult(
    List<GetAnalyticsByPeriodQueryResponse> Questions,
    OverallSatisfaction Overall,
    int SubmissionCount);
