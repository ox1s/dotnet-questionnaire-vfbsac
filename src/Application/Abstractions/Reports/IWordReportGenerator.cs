using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.GetAnalyticsByPeriods;

namespace Application.Abstractions.Reports;

public interface IWordReportGenerator
{
    Task<byte[]> GeneratePeriodReportAsync(
        string formTitle,
        DateTime periodStart,
        DateTime periodEnd,
        Dictionary<string, string> resolvedFilters,
        List<GetAnalyticsByPeriodQueryResponse> statistics,
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
