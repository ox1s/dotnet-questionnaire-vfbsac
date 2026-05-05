namespace Application.Reports.Queries.GetAnalytics;

internal sealed class ResponseMapper
{
    public AnalyticsReportResponse MapToResponse(
        FormProjection form,
        List<AnalyticsSliceResult> sliceResults)
    {
        List<AnalyticsQuestionResponse> questions = MapQuestions(form, sliceResults);
        List<AnalyticsSliceResponse> slices = MapSlices(sliceResults);

        return new AnalyticsReportResponse
        {
            FormId = form.Id,
            FormTitle = form.Title,
            Slices = slices,
            Questions = questions
        };
    }

    private static List<AnalyticsQuestionResponse> MapQuestions(
        FormProjection form,
        List<AnalyticsSliceResult> sliceResults)
    {
        List<AnalyticsQuestionResponse> questions = [];

        foreach (QuestionProjection question in form.Questions)
        {
            List<AnalyticsQuestionSliceMetricResponse> metrics = [];

            foreach (AnalyticsSliceResult sliceResult in sliceResults)
            {
                SliceQuestionMetric metric = sliceResult.QuestionMetrics.TryGetValue(
                    question.Id,
                    out SliceQuestionMetric? existingMetric)
                    ? existingMetric
                    : SliceQuestionMetric.Zero;

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

        return questions;
    }

    private static List<AnalyticsSliceResponse> MapSlices(List<AnalyticsSliceResult> sliceResults)
    {
        return sliceResults.Select(slice => new AnalyticsSliceResponse
        {
            Label = slice.Label,
            DateFrom = slice.DateFrom,
            DateTo = slice.DateTo,
            TotalSubmissions = slice.TotalSubmissions,
            OverallAverage = slice.OverallAverage,
            OverallStandardDeviation = slice.OverallStandardDeviation,
            Filters = slice.Filters
        }).ToList();
    }
}
