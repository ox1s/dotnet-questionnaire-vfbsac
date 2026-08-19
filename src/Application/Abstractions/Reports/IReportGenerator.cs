using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;

namespace Application.Abstractions.Reports;

/// <summary>
/// One worksheet to render for the single-period report: either the whole form (when no
/// discipline breakdown applies) or a single discipline/teacher slice of it.
/// </summary>
public sealed record PeriodReportSheet(string SheetName, GetAnalyticsByPeriodQueryResult AnalyticsResult);

public interface IReportGenerator
{
    Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        List<PeriodReportSheet> sheets,
        CancellationToken cancellationToken = default);

    Task<byte[]> GeneratePeriodsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByPeriodsQueryResponse> periodsData,
        CancellationToken cancellationToken = default);

    Task<byte[]> GenerateGroupsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByGroupsQueryResponse> groupsData,
        CancellationToken cancellationToken = default);
}
