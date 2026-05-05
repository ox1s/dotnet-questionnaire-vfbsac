using Domain.Questionnaires.Forms;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class MetricCalculator
{
    public SliceQuestionMetric Calculate(
        QuestionType questionType,
        QuestionAggregateProjection? aggregate)
    {
        if (aggregate is null)
        {
            return SliceQuestionMetric.Zero;
        }

        decimal resultScore = CalculateResultScore(questionType, aggregate);
        decimal standardDeviation = CalculateStandardDeviation(aggregate);

        return new SliceQuestionMetric(
            aggregate.RawAverage,
            resultScore,
            standardDeviation,
            aggregate.SubmissionCount);
    }

    public (decimal OverallAverage, decimal OverallStdDev) CalculateOverallMetrics(
        IEnumerable<SliceQuestionMetric> metrics)
    {
        var metricsList = metrics.ToList();
        var scores = metricsList.Select(m => m.ResultScore).ToList();
        var stdDevs = metricsList.Select(m => m.StandardDeviation).ToList();

        decimal overallAverage = scores.Count > 0 ? scores.Average() : 0;
        
        // Pooled standard deviation: RMS of individual stddevs
        decimal overallStdDev = stdDevs.Count > 0
            ? (decimal)Math.Sqrt(stdDevs.Average(sd => (double)(sd * sd)))
            : 0;

        return (overallAverage, overallStdDev);
    }

    private static decimal CalculateResultScore(
        QuestionType questionType,
        QuestionAggregateProjection aggregate)
    {
        if (questionType == QuestionType.WeightedRating)
        {
            return aggregate.WeightedCount > 0
                ? aggregate.WeightedNormalizedSum / aggregate.WeightedCount
                : 0;
        }

        return aggregate.RawAverage;
    }

    private static decimal CalculateStandardDeviation(QuestionAggregateProjection aggregate)
    {
        decimal variance = aggregate.RawAverageSquares - aggregate.RawAverage * aggregate.RawAverage;
        
        if (variance < -0.0001m)
        {
            // TODO: Log warning - negative variance indicates data quality issue
        }
        
        if (variance < 0m)
        {
            variance = 0;
        }

        return (decimal)Math.Sqrt((double)variance);
    }
}

internal sealed record SliceQuestionMetric(
    decimal AverageScore,
    decimal ResultScore,
    decimal StandardDeviation,
    int SubmissionCount)
{
    public static SliceQuestionMetric Zero => new(0, 0, 0, 0);
}
