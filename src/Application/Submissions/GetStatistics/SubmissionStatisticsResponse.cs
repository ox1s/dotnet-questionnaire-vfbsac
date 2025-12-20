namespace Application.Submissions.GetStatistics;

public sealed record SubmissionStatisticsResponse
{
    public Guid FormId { get; init; }
    public int TotalSubmissions { get; init; }
    public List<decimal> AverageScores { get; init; } = [];
    public List<decimal> ResultScores { get; init; } = [];
    public List<decimal> StandardDeviations { get; init; } = [];
    public decimal OverallAverage { get; init; }
    public decimal OverallStandardDeviation { get; init; }
}
