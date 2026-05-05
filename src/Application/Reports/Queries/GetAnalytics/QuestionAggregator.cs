using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class QuestionAggregator
{
    public async Task<List<QuestionAggregateProjection>> AggregateAsync(
        IQueryable<Submission> submissionsQuery,
        CancellationToken cancellationToken)
    {
        return await submissionsQuery
            .SelectMany(s => s.Answers)
            .Where(a => a.NumericValue != null)
            .GroupBy(a => a.QuestionId)
            .Select(g => new QuestionAggregateProjection(
                g.Key,
                g.Average(a => a.NumericValue!.Value),
                g.Average(a => a.NumericValue!.Value * a.NumericValue!.Value),
                g.Sum(a => a.Weight.HasValue && a.Weight.Value > 0 ? a.NumericValue!.Value / a.Weight.Value * 10 : 0),
                g.Average(a => a.Weight.HasValue && a.Weight.Value > 0 
                    ? a.NumericValue!.Value / a.Weight.Value * 10 * a.NumericValue!.Value / a.Weight.Value * 10
                    : 0),
                g.Sum(a => a.Weight.HasValue && a.Weight.Value > 0 ? 1 : 0),
                g.Count()))
            .ToListAsync(cancellationToken);
    }
}

internal sealed record QuestionAggregateProjection(
    Guid QuestionId,
    decimal RawAverage,
    decimal RawAverageSquares,
    decimal WeightedNormalizedSum,
    decimal WeightedNormalizedAverageSquares,
    int WeightedCount,
    int SubmissionCount);
