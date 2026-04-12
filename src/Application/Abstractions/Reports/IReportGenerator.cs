using Application.Reports.Queries.GetAnalytics;

namespace Application.Abstractions.Reports;

public interface IReportGenerator
{
    Task<byte[]> GenerateAnalyticsReport(AnalyticsReportResponse analyticsReport, CancellationToken cancellationToken = default);
}
