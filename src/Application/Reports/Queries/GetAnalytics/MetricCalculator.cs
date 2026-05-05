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
        decimal standardDeviation = CalculateStandardDeviation(questionType, aggregate);

        return new SliceQuestionMetric(
            aggregate.RawAverage,
            resultScore,
            standardDeviation,
            aggregate.SubmissionCount);
    }

    public (decimal OverallAverage, decimal OverallStdDev) CalculateOverallMetrics(
        IEnumerable<SliceQuestionMetric> metrics)
    {
        var metricsWithData = metrics.Where(m => m.SubmissionCount > 0).ToList();
        
        if (metricsWithData.Count == 0)
        {
            return (0, 0);
        }

        var scores = metricsWithData.Select(m => m.ResultScore).ToList();
        var stdDevs = metricsWithData.Select(m => m.StandardDeviation).ToList();

        decimal overallAverage = scores.Average();
        
        // Pooled standard deviation: RMS of individual stddevs
        decimal overallStdDev = (decimal)Math.Sqrt(stdDevs.Average(sd => (double)(sd * sd)));

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

    private static decimal CalculateStandardDeviation(
        QuestionType questionType,
        QuestionAggregateProjection aggregate)
    {
        decimal average;
        decimal averageSquares;

        if (questionType == QuestionType.WeightedRating && aggregate.WeightedCount > 0)
        {
            average = aggregate.WeightedNormalizedSum / aggregate.WeightedCount;
            averageSquares = aggregate.WeightedNormalizedAverageSquares;
        }
        else
        {
            average = aggregate.RawAverage;
            averageSquares = aggregate.RawAverageSquares;
        }

        decimal variance = averageSquares - average * average;
        
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
