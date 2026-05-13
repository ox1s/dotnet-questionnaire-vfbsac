using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;

namespace Application.Abstractions.Reports;

public interface IWordReportGenerator
{
    /// <summary>
    /// Generates a Word document for a single period analytics report
    /// </summary>
    Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        List<GetAnalyticsByPeriodQueryResponse> statistics,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Word document for multi-period comparison report
    /// </summary>
    Task<byte[]> GeneratePeriodsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByPeriodsQueryResponse> periodsData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Word document for groups comparison report
    /// </summary>
    Task<byte[]> GenerateGroupsComparisonReportAsync(
        string formTitle,
        List<GetAnalyticsByGroupsQueryResponse> groupsData,
        CancellationToken cancellationToken = default);
}
