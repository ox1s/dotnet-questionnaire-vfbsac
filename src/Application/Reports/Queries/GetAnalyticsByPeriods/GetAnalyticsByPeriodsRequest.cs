using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByPeriods;

public sealed record GetAnalyticsByPeriodsRequest(
    string Label,
    DateTime DateFrom,
    DateTime DateTo,
    AnalyticsFilterSet FilterSet);
