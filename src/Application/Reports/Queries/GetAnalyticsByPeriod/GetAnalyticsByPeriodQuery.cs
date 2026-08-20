using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetAnalyticsByPeriod;

public sealed record GetAnalyticsByPeriodQuery(
    Guid FormId,
    DateTime FromDate,
    DateTime ToDate,
    AnalyticsFilterSet FilterSet)
    : IQuery<GetAnalyticsByPeriodQueryResult>;
