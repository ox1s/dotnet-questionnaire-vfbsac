using Application.Abstractions.Data;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class AnalyticsReportBuilder(IApplicationDbContext context)
    : IAnalyticsReportBuilder
{
    public async Task<AnalyticsReportResponse> BuildAsync(
        Guid formId,
        IReadOnlyCollection<AnalyticsSliceRequest> slices,
        CancellationToken cancellationToken)
    {
        if (slices.Count == 0)
        {
            throw new InvalidOperationException("At least one analytics slice is required.");
        }

        // 1. Берем форму с вопросами, исключая текст
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
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Form {formId} was not found.");

        List<AnalyticsSliceResult> sliceResults = [];

        Dictionary<Guid, string> departmentNames = await context.Departments
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        Dictionary<Guid, string> disciplineNames = await context.Disciplines
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        Dictionary<Guid, string> specialityNames = await context.Specialities
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        Dictionary<Guid, string> specializationNames = await context.Specializations
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        Dictionary<Guid, string> teacherNames = await context.Teachers
            .AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => item.FullName, cancellationToken);

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

        return new AnalyticsReportResponse
        {
            FormId = form.Id,
            FormTitle = form.Title,
            Slices = sliceResults
                .Select(slice => new AnalyticsSliceResponse
                {
                    Label = slice.Label,
                    DateFrom = slice.DateFrom,
                    DateTo = slice.DateTo,
                    TotalSubmissions = slice.TotalSubmissions,
                    OverallAverage = slice.OverallAverage,
                    OverallStandardDeviation = slice.OverallStandardDeviation,
                    Filters = slice.Filters,
                    FilterDisplay = BuildFilterDisplay(
                        slice.Filters,
                        disciplineNames,
                        teacherNames,
                        departmentNames,
                        specialityNames,
                        specializationNames)
                })
                .ToList(),
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
                QuestionId: g.Key,
                RawAverage: g.Average(a => a.NumericValue!.Value),
                RawAverageSquares: g.Average(a => a.NumericValue!.Value * a.NumericValue!.Value),
                WeightedNormalizedSum: g.Sum(a => a.Weight.HasValue && a.Weight.Value > 0 ?
                                                    a.NumericValue!.Value / a.Weight.Value * 10
                                                    : 0),
                WeightedCount: g.Sum(a => a.Weight.HasValue && a.Weight.Value > 0 ?
                                            1 : 0),
                SubmissionCount: g.Count()))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, SliceQuestionMetricProjection> metricsByQuestionId = [];
        List<decimal> overallScores = [];
        List<decimal> overallStandardDeviations = [];

        foreach (QuestionProjection question in form.Questions)
        {
            // Сырые данные в бд для конкретного вопроса
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
                if (variance < 0)
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
        decimal overallStdDev = overallStandardDeviations.Count > 0 ? overallStandardDeviations.Average() : 0;

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

    private static AnalyticsFilterDisplaySet BuildFilterDisplay(
        AnalyticsFilterSet filters,
        IReadOnlyDictionary<Guid, string> disciplineNames,
        IReadOnlyDictionary<Guid, string> teacherNames,
        IReadOnlyDictionary<Guid, string> departmentNames,
        IReadOnlyDictionary<Guid, string> specialityNames,
        IReadOnlyDictionary<Guid, string> specializationNames)
    {
        return new AnalyticsFilterDisplaySet(
            Discipline: ResolveName(filters.DisciplineId, disciplineNames),
            Teacher: ResolveName(filters.TeacherId, teacherNames),
            Department: ResolveName(filters.DepartmentId, departmentNames),
            Speciality: ResolveName(filters.SpecialityId, specialityNames),
            Specialization: ResolveName(filters.SpecializationId, specializationNames),
            Organization: string.IsNullOrWhiteSpace(filters.OrganizationName)
                ? null
                : filters.OrganizationName);
    }

    private static string? ResolveName(
        Guid? id,
        IReadOnlyDictionary<Guid, string> names)
    {
        if (!id.HasValue)
        {
            return null;
        }

        return names.TryGetValue(id.Value, out string? name)
            ? name
            : id.Value.ToString();
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

    /// <summary>
    /// Промежуточная проекция для агрегации метрик по вопросам в рамках одного среза.
    /// </summary>
    /// <param name="RawAverage">Cреднее значение</param>
    /// <param name="RawAverageSquares">Квадраты средних значений</param>
    /// <param name="WeightedNormalizedSum">Сумма нормализованных взвешенных (относительно весов) значений для рейтингов с весами</param>
    /// <param name="WeightedCount">Количество ответов, учитываемых в взвешенном среднем (для рейтингов с весами)</param>
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
