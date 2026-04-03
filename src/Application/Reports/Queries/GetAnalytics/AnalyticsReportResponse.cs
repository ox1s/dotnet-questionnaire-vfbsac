namespace Application.Reports.Queries.GetAnalytics;

public sealed record AnalyticsReportResponse
{
    public Guid FormId { get; init; }
    public string FormTitle { get; init; } = string.Empty;
    public List<AnalyticsSliceResponse> Slices { get; init; } = [];
    public List<AnalyticsQuestionResponse> Questions { get; init; } = [];
}

public sealed record AnalyticsSliceResponse
{
    public string Label { get; init; } = string.Empty;
    public DateTime DateFrom { get; init; }
    public DateTime DateTo { get; init; }
    public int TotalSubmissions { get; init; }
    public decimal OverallAverage { get; init; }
    public decimal OverallStandardDeviation { get; init; }
    public AnalyticsFilterSet Filters { get; init; } = new();
    public AnalyticsFilterDisplaySet FilterDisplay { get; init; } = new();
}

public sealed record AnalyticsQuestionResponse
{
    public Guid QuestionId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public string QuestionType { get; init; } = string.Empty;
    public int Order { get; init; }
    public List<AnalyticsQuestionSliceMetricResponse> SliceMetrics { get; init; } = [];
}

public sealed record AnalyticsQuestionSliceMetricResponse
{
    public string SliceLabel { get; init; } = string.Empty;
    public decimal AverageScore { get; init; }
    public decimal ResultScore { get; init; }
    public decimal StandardDeviation { get; init; }
    public int SubmissionCount { get; init; }
}

public sealed record AnalyticsFilterDisplaySet(
    string? Discipline = null,
    string? Teacher = null,
    string? Department = null,
    string? Speciality = null,
    string? Specialization = null,
    string? Organization = null);
