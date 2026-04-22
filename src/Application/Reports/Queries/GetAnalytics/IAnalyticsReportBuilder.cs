using SharedKernel;

namespace Application.Reports.Queries.GetAnalytics;

internal interface IAnalyticsReportBuilder
{
    Task<Result<AnalyticsReportResponse>> BuildAsync(
        Guid formId,
        IReadOnlyCollection<AnalyticsSliceRequest> slices,
        CancellationToken cancellationToken);
}
