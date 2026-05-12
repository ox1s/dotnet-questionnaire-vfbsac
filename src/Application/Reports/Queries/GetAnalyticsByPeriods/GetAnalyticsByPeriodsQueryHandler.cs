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

        // Step 3: Get submission IDs from DB
        List<Guid> submissionIds = await submissionsQuery
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (submissionIds.Count == 0)
        {
            return new GetAnalyticsByPeriodsQueryResponse(
                Label: period.Label,
                PeriodStart: period.DateFrom,
                PeriodEnd: period.DateTo,
                QuestionStatistics: []);
        }

        // Step 4: Get numeric answers grouped by question from DB
        var answersGrouped = await context.Answers
            .AsNoTracking()
            .Where(a => submissionIds.Contains(a.SubmissionId) &&
                       a.Value == null &&
                       a.NumericValue != null)
            .GroupBy(a => a.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Values = g.Select(a => a.NumericValue!.Value).ToList()
            })
            .ToListAsync(cancellationToken);

        if (answersGrouped.Count == 0)
        {
            return new GetAnalyticsByPeriodsQueryResponse(
                Label: period.Label,
                PeriodStart: period.DateFrom,
                PeriodEnd: period.DateTo,
                QuestionStatistics: []);
        }

        // Step 5: Load question texts from DB
        var questionIds = answersGrouped.Select(g => g.QuestionId).ToList();

        Dictionary<Guid, string> questions = await context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => new { q.Id, q.Text })
            .ToDictionaryAsync(q => q.Id, q => q.Text, cancellationToken);

        // Step 6: Calculate statistics in-memory (median/mode/std not available in SQL)
        var questionStats = answersGrouped
            .Select(group => new QuestionStatistics(
                QuestionId: group.QuestionId,
                QuestionText: questions.GetValueOrDefault(group.QuestionId) ?? string.Empty,
                Median: StatisticsCalculator.CalculateMedian(group.Values),
                Mean: StatisticsCalculator.CalculateMean(group.Values),
                Mode: StatisticsCalculator.CalculateMode(group.Values),
                StandardDeviation: StatisticsCalculator.CalculateStandardDeviation(group.Values),
                ResponseCount: group.Values.Count))
            .ToList();

        return new GetAnalyticsByPeriodsQueryResponse(
            Label: period.Label,
            PeriodStart: period.DateFrom,
            PeriodEnd: period.DateTo,
            QuestionStatistics: questionStats);
    }
}
