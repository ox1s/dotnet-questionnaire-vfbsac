namespace Application.Reports.Queries.Shared;

internal static class StatisticsCalculator
{
    public static decimal CalculateMedian(List<decimal> values)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;

        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2
            : sorted[mid];
    }

    public static decimal CalculateMean(List<decimal> values)
    {
        return values.Count == 0 ? 0 : values.Average();
    }

    public static decimal CalculateMode(List<decimal> values)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        return values
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    public static decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        decimal mean = CalculateMean(values);
        decimal sumOfSquares = values.Sum(v => (v - mean) * (v - mean));
        
        return (decimal)Math.Sqrt((double)(sumOfSquares / values.Count));
    }
}
