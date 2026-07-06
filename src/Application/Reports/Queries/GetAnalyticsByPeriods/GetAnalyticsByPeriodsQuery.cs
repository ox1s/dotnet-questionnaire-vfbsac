using Application.Abstractions.Messaging;

namespace Application.Reports.Queries.GetAnalyticsByPeriods;

public sealed record GetAnalyticsByPeriodsQuery(
    Guid FormId,
    List<GetAnalyticsByPeriodsRequest> Periods)
    : IQuery<List<GetAnalyticsByPeriodsQueryResponse>>;
