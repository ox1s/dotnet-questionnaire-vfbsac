using Application.Abstractions.Data;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class AnalyticsReportBuilder(
    IApplicationDbContext context,
    SubmissionQueryBuilder queryBuilder,
    QuestionAggregator aggregator,
    MetricCalculator calculator,
    ResponseMapper mapper)
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

        FormProjection? form = await LoadFormAsync(formId, cancellationToken);

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

        return mapper.MapToResponse(form, sliceResults);
    }

    private async Task<FormProjection?> LoadFormAsync(Guid formId, CancellationToken cancellationToken)
    {
        return await context.Forms
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
    }

    private async Task<AnalyticsSliceResult> BuildSliceAsync(
        FormProjection form,
        AnalyticsSliceRequest slice,
        CancellationToken cancellationToken)
    {
        AnalyticsFilterSet filters = slice.ToFilterSet();
        
        IQueryable<Submission> submissionsQuery = queryBuilder.BuildQuery(
            form.Id,
            slice.DateFrom,
            slice.DateTo,
            filters);

        int totalSubmissions = await submissionsQuery.CountAsync(cancellationToken);

        List<QuestionAggregateProjection> aggregates = await aggregator.AggregateAsync(
            submissionsQuery,
            cancellationToken);

        var aggregatesByQuestionId = aggregates.ToDictionary(a => a.QuestionId);

        Dictionary<Guid, SliceQuestionMetric> metricsByQuestionId = [];

        foreach (QuestionProjection question in form.Questions)
        {
            aggregatesByQuestionId.TryGetValue(question.Id, out QuestionAggregateProjection? aggregate);
            
            SliceQuestionMetric metric = calculator.Calculate(question.Type, aggregate);
            metricsByQuestionId[question.Id] = metric;
        }

        (decimal overallAverage, decimal overallStdDev) = calculator.CalculateOverallMetrics(
            metricsByQuestionId.Values);

        DateTime normalizedFrom = slice.DateFrom.Date;
        DateTime normalizedToExclusive = slice.DateTo.Date.AddDays(1);

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
}
