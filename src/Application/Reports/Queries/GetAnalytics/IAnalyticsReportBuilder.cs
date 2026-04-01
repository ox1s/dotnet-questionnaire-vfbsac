namespace Application.Reports.Queries.GetAnalytics;

internal interface IAnalyticsReportBuilder
{
    Task<AnalyticsReportResponse> BuildAsync(
        Guid formId,
        IReadOnlyCollection<AnalyticsSliceRequest> slices,
        CancellationToken cancellationToken);
}
