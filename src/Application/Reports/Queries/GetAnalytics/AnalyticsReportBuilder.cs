using Application.Abstractions.Data;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class AnalyticsReportBuilder(IApplicationDbContext context)
    : IAnalyticsReportBuilder
{
    public async Task<Result<AnalyticsReportResponse>> BuildAsync(
        Guid formId,
        IReadOnlyCollection<AnalyticsSliceRequest> slices,
        CancellationToken cancellationToken)
    {
        if (slices.Count == 0)
        {
            return Result.Failure<AnalyticsReportResponse>(
                Error.Validation("Analytics.SlicesRequired", "At least one analytics slice is required."));
        }

        FormProjection form = await context.Forms
            .AsNoTracking()
            .Where(f => f.Id == formId)
            .Select(f => new FormProjection(
                            f.Id,
                            f.Title,
                            f.Questions
                                .Where(q =>
                                    q.Type == QuestionType.Number ||
                                    q.Type == QuestionType.Rating ||
                                    q.Type == QuestionType.WeightedRating)
                                .OrderBy(q => q.Order)
                                .Select(q => new QuestionProjection(q.Id, q.Text, q.Type, q.Order))
                                .ToList()))
                                  .FirstOrDefaultAsync(cancellationToken);

        if (form == null)
        {
            return Result.Failure<AnalyticsReportResponse>(FormErrors.NotFound(formId));
        }

        List<AnalyticsSliceResult> sliceResults = [];
        foreach (AnalyticsSliceRequest slice in slices)
        {
            AnalyticsSliceResult sliceResult = await BuildSliceAsync(form, slice, cancellationToken);
            sliceResults.Add(sliceResult);
        }

        List<AnalyticsQuestionResponse> questions = [];
        foreach (QuestionProjection question in form.Questions)
        {
            List<AnalyticsQuestionSliceMetricResponse> metrics = [];

            foreach (AnalyticsSliceResult sliceResult in sliceResults)
            {
                SliceQuestionMetricProjection metric = sliceResult.QuestionMetrics.TryGetValue(
                    question.Id,
                    out SliceQuestionMetricProjection? existingMetric)
                    ? existingMetric
                    : SliceQuestionMetricProjection.Zero;

                metrics.Add(new AnalyticsQuestionSliceMetricResponse
                {
                    SliceLabel = sliceResult.Label,
                    AverageScore = metric.AverageScore,
                    ResultScore = metric.ResultScore,
                    StandardDeviation = metric.StandardDeviation,
                    SubmissionCount = metric.SubmissionCount
                });
            }

            questions.Add(new AnalyticsQuestionResponse
            {
                QuestionId = question.Id,
                QuestionText = question.Text,
                QuestionType = question.Type.ToString(),
                Order = question.Order,
                SliceMetrics = metrics
            });
        }

        var list = sliceResults.Select(slice => new AnalyticsSliceResponse
        {
            Label = slice.Label,
            DateFrom = slice.DateFrom,
            DateTo = slice.DateTo,
            TotalSubmissions = slice.TotalSubmissions,
            OverallAverage = slice.OverallAverage,
            OverallStandardDeviation = slice.OverallStandardDeviation,
            Filters = slice.Filters
        })
            .ToList();

        return new AnalyticsReportResponse
        {
            FormId = form.Id,
            FormTitle = form.Title,
            Slices = list,
            Questions = questions
        };
    }

    private async Task<AnalyticsSliceResult> BuildSliceAsync(
        FormProjection form,
        AnalyticsSliceRequest slice,
        CancellationToken cancellationToken)
    {
        DateTime normalizedFrom = slice.DateFrom.Date;
        DateTime normalizedToExclusive = slice.DateTo.Date.AddDays(1);

        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s =>
                s.FormId == form.Id &&
                s.SubmittedAt >= normalizedFrom &&
                s.SubmittedAt < normalizedToExclusive);

        AnalyticsFilterSet filters = slice.ToFilterSet();
        submissionsQuery = ApplyFilters(submissionsQuery, filters);

        int totalSubmissions = await submissionsQuery.CountAsync(cancellationToken);

        List<QuestionAggregateProjection> aggregates = await submissionsQuery
            .SelectMany(s => s.Answers)
            .Where(a => a.NumericValue != null)
            .GroupBy(a => a.QuestionId)
            .Select(g => new QuestionAggregateProjection(
                g.Key,
                g.Average(a => a.NumericValue!.Value),
                g.Average(a => a.NumericValue!.Value * a.NumericValue!.Value),
                g.Sum(a => a.Weight.HasValue && a.Weight.Value > 0 ? a.NumericValue!.Value / a.Weight.Value * 10 : 0),
                g.Sum(a => a.Weight.HasValue && a.Weight.Value > 0 ? 1 : 0),
                g.Count()))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, SliceQuestionMetricProjection> metricsByQuestionId = [];
        List<decimal> overallScores = [];
        List<decimal> overallStandardDeviations = [];

        foreach (QuestionProjection question in form.Questions)
        {
            QuestionAggregateProjection? aggregate = aggregates.FirstOrDefault(a => a.QuestionId == question.Id);
            SliceQuestionMetricProjection metric;

            if (aggregate is null)
            {
                metric = SliceQuestionMetricProjection.Zero;
            }
            else
            {
                decimal resultScore = aggregate.RawAverage;
                if (question.Type == QuestionType.WeightedRating)
                {
                    resultScore = aggregate.WeightedCount > 0
                        ? aggregate.WeightedNormalizedSum / aggregate.WeightedCount
                        : 0;
                }

                decimal variance = aggregate.RawAverageSquares - aggregate.RawAverage * aggregate.RawAverage;
                if (variance < -0.0001m)
                {
                    // Log warning: negative variance indicates data quality issue
                    variance = 0;
                }
                else if (variance < 0)
                {
                    variance = 0;
                }

                decimal standardDeviation = (decimal)Math.Sqrt((double)variance);

                metric = new SliceQuestionMetricProjection(
                    aggregate.RawAverage,
                    resultScore,
                    standardDeviation,
                    aggregate.SubmissionCount);
            }

            metricsByQuestionId[question.Id] = metric;
            overallScores.Add(metric.ResultScore);
            overallStandardDeviations.Add(metric.StandardDeviation);
        }

        decimal overallAverage = overallScores.Count > 0 ? overallScores.Average() : 0;
        
        // Calculate pooled standard deviation (root-mean-square of individual stddevs)
        decimal overallStdDev = overallStandardDeviations.Count > 0 
            ? (decimal)Math.Sqrt(overallStandardDeviations.Average(sd => (double)(sd * sd)))
            : 0;

        return new AnalyticsSliceResult(
            slice.Label,
            normalizedFrom,
            normalizedToExclusive.AddTicks(-1),
            filters,
            totalSubmissions,
            overallAverage,
            overallStdDev,
            metricsByQuestionId);
    }

    private static IQueryable<Submission> ApplyFilters(
        IQueryable<Submission> submissionsQuery,
        AnalyticsFilterSet filters)
    {
        if (filters.DisciplineId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.DisciplineId == filters.DisciplineId);
        }

        if (filters.TeacherId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.TeacherId == filters.TeacherId);
        }

        if (filters.DepartmentId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.DepartmentId == filters.DepartmentId);
        }

        if (filters.SpecialityId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.SpecialityId == filters.SpecialityId);
        }

        if (filters.SpecializationId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.SpecializationId == filters.SpecializationId);
        }

        if (!string.IsNullOrWhiteSpace(filters.OrganizationName))
        {
            submissionsQuery = submissionsQuery
                .Where(s =>
                    s.Context.OrganizationName != null &&
                    s.Context.OrganizationName.Contains(filters.OrganizationName));
        }

        return submissionsQuery;
    }

    private sealed record FormProjection(
        Guid Id,
        string Title,
        List<QuestionProjection> Questions);

    private sealed record QuestionProjection(
        Guid Id,
        string Text,
        QuestionType Type,
        int Order);

    private sealed record QuestionAggregateProjection(
        Guid QuestionId,
        decimal RawAverage,
        decimal RawAverageSquares,
        decimal WeightedNormalizedSum,
        int WeightedCount,
        int SubmissionCount);

    private sealed record AnalyticsSliceResult(
        string Label,
        DateTime DateFrom,
        DateTime DateTo,
        AnalyticsFilterSet Filters,
        int TotalSubmissions,
        decimal OverallAverage,
        decimal OverallStandardDeviation,
        Dictionary<Guid, SliceQuestionMetricProjection> QuestionMetrics);

    private sealed record SliceQuestionMetricProjection(
        decimal AverageScore,
        decimal ResultScore,
        decimal StandardDeviation,
        int SubmissionCount)
    {
        public static SliceQuestionMetricProjection Zero => new(0, 0, 0, 0);
    }
}
