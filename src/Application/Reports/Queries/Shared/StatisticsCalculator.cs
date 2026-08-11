namespace Application.Reports.Queries.Shared;

/// <summary>
/// Implements the calculation from "Методика оценки удовлетворенности потребителей"
/// (college-specs/Методика_оценки.md, section 5.6, formulas 1-3): per-criterion consumer
/// satisfaction is the mean of each respondent's (score / importance weight) ratio, scaled to
/// a percentage, while the spread is the population standard deviation of the raw scores.
/// </summary>
internal static class StatisticsCalculator
{
    /// <summary>
    /// Formula (1): УП_j = mean(ОУ_ij / В_ij) x 100, %. Respondents without a stated importance
    /// weight (<paramref name="ratings"/> weight of 0) are excluded from the ratio to avoid a
    /// division by zero.
    /// </summary>
    public static decimal CalculateSatisfactionPercentage(List<(decimal Score, decimal Weight)> ratings)
    {
        var ratios = ratings
            .Where(r => r.Weight > 0)
            .Select(r => r.Score / r.Weight)
            .ToList();

        return ratios.Count == 0 ? 0 : ratios.Average() * 100;
    }

    /// <summary>Formula (2): mean of the raw satisfaction scores, баллы.</summary>
    public static decimal CalculateAverageScore(List<decimal> scores)
    {
        return scores.Count == 0 ? 0 : scores.Average();
    }

    /// <summary>Formula (3): population standard deviation of the raw satisfaction scores.</summary>
    public static decimal CalculateStandardDeviation(List<decimal> scores)
    {
        if (scores.Count == 0)
        {
            return 0;
        }

        decimal mean = CalculateAverageScore(scores);
        decimal sumOfSquares = scores.Sum(v => (v - mean) * (v - mean));

        return (decimal)Math.Sqrt((double)(sumOfSquares / scores.Count));
    }

    /// <summary>Table 1 classification of a satisfaction percentage.</summary>
    public static SatisfactionRating ClassifySatisfaction(decimal satisfactionPercentage)
    {
        return satisfactionPercentage switch
        {
            < 40 => SatisfactionRating.Unsatisfactory,
            < 60 => SatisfactionRating.Satisfactory,
            < 80 => SatisfactionRating.Good,
            _ => SatisfactionRating.Excellent
        };
    }
}
