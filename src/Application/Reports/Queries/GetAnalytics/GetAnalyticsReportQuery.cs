using Application.Abstractions.Messaging;

namespace Application.Reports.Queries.GetAnalytics;

public sealed record GetAnalyticsReportQuery(
    Guid FormId,
    List<AnalyticsSliceRequest> Slices)
    : IQuery<AnalyticsReportResponse>;
