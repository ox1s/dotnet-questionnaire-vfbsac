namespace Application.Reports.Queries.GetAnalytics;

internal sealed record AnalyticsSliceResult(
    string Label,
    DateTime DateFrom,
    DateTime DateTo,
    AnalyticsFilterSet Filters,
    int TotalSubmissions,
    decimal OverallAverage,
    decimal OverallStandardDeviation,
    Dictionary<Guid, SliceQuestionMetric> QuestionMetrics);
