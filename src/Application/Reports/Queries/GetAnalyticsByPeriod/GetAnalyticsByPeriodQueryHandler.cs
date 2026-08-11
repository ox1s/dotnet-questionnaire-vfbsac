using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalyticsByPeriod;

/// <summary>
/// Handles analytics query for a specific time period.
/// Returns consumer-satisfaction metrics (satisfaction %, average score, standard deviation, rating)
/// for each numeric question, per "Методика оценки удовлетворенности потребителей".
/// </summary>
internal sealed class GetAnalyticsByPeriodQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetAnalyticsByPeriodQuery, List<GetAnalyticsByPeriodQueryResponse>>
{
    public async Task<Result<List<GetAnalyticsByPeriodQueryResponse>>> Handle(
        GetAnalyticsByPeriodQuery query,
        CancellationToken cancellationToken)
    {
        // Step 1: Verify form exists
        bool formExists = await context.Forms
            .AnyAsync(f => f.Id == query.FormId, cancellationToken);

        if (!formExists)
        {
            return Result.Failure<List<GetAnalyticsByPeriodQueryResponse>>(
                FormErrors.NotFound(query.FormId));
        }

        // Step 2: Get all submissions for the form
        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s => s.FormId == query.FormId);

        // Step 3: Apply filters (department, discipline, teacher, etc.)
        IQueryable<Submission> filteredQuery = SubmissionFilterHelper.ApplyFilters(
            submissionsQuery,
            query.FilterSet);

        // Step 4: Filter by date range (inclusive end date by adding 1 day)
        DateTime normalizedToDate = query.ToDate.AddDays(1);
        
        IQueryable<Submission> filteredByDate = filteredQuery
            .Where(s => s.SubmittedAt >= query.FromDate && s.SubmittedAt < normalizedToDate);

        // Step 5: Get numeric answers grouped by question
        // Only select answers with NumericValue (Number, WeightedRating question types)
        // Weight defaults to 10 (max importance) when the question has no explicit importance weight
        var answersGrouped = await context.Answers
            .AsNoTracking()
            .Where(a => filteredByDate.Select(s => s.Id).Contains(a.SubmissionId) &&
                        a.Value == null &&
                        a.NumericValue != null)
            .GroupBy(a => a.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Ratings = g.Select(a => new { Score = a.NumericValue!.Value, Weight = a.Weight ?? 10m }).ToList()
            }).ToListAsync(cancellationToken);

        if (answersGrouped.Count == 0)
        {
            return new List<GetAnalyticsByPeriodQueryResponse>();
        }

        // Step 6: Get question texts from database
        IEnumerable<Guid> questionIds = answersGrouped.Select(g => g.QuestionId);

        var questions = await context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => new { q.Id, q.Text })
            .ToListAsync(cancellationToken);

        var questionDict = questions.ToDictionary(q => q.Id, q => q.Text);

        // Step 7: Calculate statistics for each question in-memory
        var responses = answersGrouped
            .Select(group =>
            {
                var scores = group.Ratings.Select(r => r.Score).ToList();
                var ratioPairs = group.Ratings.Select(r => (r.Score, r.Weight)).ToList();
                decimal satisfactionPercentage = StatisticsCalculator.CalculateSatisfactionPercentage(ratioPairs);

                return new GetAnalyticsByPeriodQueryResponse(
                    QuestionId: group.QuestionId,
                    QuestionText: questionDict.GetValueOrDefault(group.QuestionId) ?? string.Empty,
                    SatisfactionPercentage: satisfactionPercentage,
                    AverageScore: StatisticsCalculator.CalculateAverageScore(scores),
                    StandardDeviation: StatisticsCalculator.CalculateStandardDeviation(scores),
                    Rating: StatisticsCalculator.ClassifySatisfaction(satisfactionPercentage),
                    ResponseCount: scores.Count);
            })
            .ToList();

        return responses;
    }
}
