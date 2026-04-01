using Application.Reports.Queries.GetAnalytics;

namespace Application.Abstractions.Reports;

public interface IReportGenerator
{
    byte[] GenerateAnalyticsReport(AnalyticsReportResponse analyticsReport);
}
