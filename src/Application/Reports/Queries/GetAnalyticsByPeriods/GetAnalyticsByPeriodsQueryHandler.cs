using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalyticsByPeriods;

/// <summary>
/// Handles analytics query for multiple user-defined time periods.
/// Returns statistical metrics for each question within each specified period.
/// </summary>
internal sealed class GetAnalyticsByPeriodsQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetAnalyticsByPeriodsQuery, List<GetAnalyticsByPeriodsQueryResponse>>
{
    public async Task<Result<List<GetAnalyticsByPeriodsQueryResponse>>> Handle(
        GetAnalyticsByPeriodsQuery query,
        CancellationToken cancellationToken)
    {
        // Step 1: Verify form exists
        bool formExists = await context.Forms
            .AnyAsync(f => f.Id == query.FormId, cancellationToken);

        if (!formExists)
        {
            return Result.Failure<List<GetAnalyticsByPeriodsQueryResponse>>(
                FormErrors.NotFound(query.FormId));
        }

        if (query.Periods.Count == 0)
        {
            return new List<GetAnalyticsByPeriodsQueryResponse>();
        }

        // Step 2: Process each period with separate DB query for maximum DB-side filtering
        List<GetAnalyticsByPeriodsQueryResponse> responses = new();

        foreach (GetAnalyticsByPeriodsRequest period in query.Periods)
        {
            GetAnalyticsByPeriodsQueryResponse response = await ProcessPeriod(
                query.FormId,
                period,
                cancellationToken);

            responses.Add(response);
        }

        return responses;
    }

    private async Task<GetAnalyticsByPeriodsQueryResponse> ProcessPeriod(
        Guid formId,
        GetAnalyticsByPeriodsRequest period,
        CancellationToken cancellationToken)
    {
        // Step 1: Build submission query with filters applied in DB
        DateTime normalizedToDate = period.DateTo.AddDays(1);
        
        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s => s.FormId == formId &&
                       s.SubmittedAt >= period.DateFrom &&
                       s.SubmittedAt < normalizedToDate);

        // Step 2: Apply filters in DB query
        submissionsQuery = SubmissionFilterHelper.ApplyFilters(submissionsQuery, period.FilterSet);

        // Step 2b: Distinct count of submissions matching the filters/date range, independent of
        // whether they contain numeric answers.
        int submissionCount = await submissionsQuery.CountAsync(cancellationToken);

        // Step 3: Use submission query as subquery (no materialization)
        IQueryable<Guid> submissionIdsQuery = submissionsQuery.Select(s => s.Id);

        // Step 4: Get numeric answers grouped by question from DB
        // Weight defaults to 10 (max importance) when the question has no explicit importance weight
        var answersGrouped = await context.Answers
            .AsNoTracking()
            .Where(a => submissionIdsQuery.Contains(a.SubmissionId) &&
                       a.Value == null &&
                       a.NumericValue != null)
            .GroupBy(a => a.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Ratings = g.Select(a => new { Score = a.NumericValue!.Value, Weight = a.Weight ?? 10m }).ToList()
            })
            .ToListAsync(cancellationToken);

        if (answersGrouped.Count == 0)
        {
            return new GetAnalyticsByPeriodsQueryResponse(
                Label: period.Label,
                PeriodStart: period.DateFrom,
                PeriodEnd: period.DateTo,
                QuestionStatistics: [],
                Overall: StatisticsCalculator.CalculateOverallSatisfaction([], []),
                SubmissionCount: submissionCount);
        }

        // Step 5: Load question texts from DB
        var questionIds = answersGrouped.Select(g => g.QuestionId).ToList();

        Dictionary<Guid, string> questions = await context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => new { q.Id, q.Text })
            .ToDictionaryAsync(q => q.Id, q => q.Text, cancellationToken);

        // Step 6: Calculate statistics in-memory (not available in SQL)
        var questionStats = answersGrouped
            .Select(group =>
            {
                var scores = group.Ratings.Select(r => r.Score).ToList();
                var ratioPairs = group.Ratings.Select(r => (r.Score, r.Weight)).ToList();
                decimal satisfactionPercentage = StatisticsCalculator.CalculateSatisfactionPercentage(ratioPairs);

                return new QuestionStatistics(
                    QuestionId: group.QuestionId,
                    QuestionText: questions.GetValueOrDefault(group.QuestionId) ?? string.Empty,
                    SatisfactionPercentage: satisfactionPercentage,
                    AverageScore: StatisticsCalculator.CalculateAverageScore(scores),
                    StandardDeviation: StatisticsCalculator.CalculateStandardDeviation(scores),
                    Rating: StatisticsCalculator.ClassifySatisfaction(satisfactionPercentage),
                    ResponseCount: scores.Count);
            })
            .ToList();

        OverallSatisfaction overall = StatisticsCalculator.CalculateOverallSatisfaction(
            questionStats.Select(s => s.SatisfactionPercentage).ToList(),
            questionStats.Select(s => s.StandardDeviation).ToList());

        return new GetAnalyticsByPeriodsQueryResponse(
            Label: period.Label,
            PeriodStart: period.DateFrom,
            PeriodEnd: period.DateTo,
            QuestionStatistics: questionStats,
            Overall: overall,
            SubmissionCount: submissionCount);
    }
}
