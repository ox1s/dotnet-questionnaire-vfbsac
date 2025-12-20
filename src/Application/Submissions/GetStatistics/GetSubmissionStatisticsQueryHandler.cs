using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;
using Domain.Questionnaires.SubmissionAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Submissions.GetStatistics;

internal sealed class GetSubmissionStatisticsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSubmissionStatisticsQuery, SubmissionStatisticsResponse>
{
    public async Task<Result<SubmissionStatisticsResponse>> Handle(
        GetSubmissionStatisticsQuery query,
        CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f => f.Id == query.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure<SubmissionStatisticsResponse>(FormErrors.NotFound(query.FormId));
        }

        IQueryable<Submission> submissionsQuery = context.Submissions
            .Include(s => s.Answers)
            .Where(s => s.FormId == query.FormId);

        if (query.DisciplineId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.DisciplineId == query.DisciplineId);
        }

        if (query.TeacherId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.TeacherId == query.TeacherId);
        }

        if (query.DepartmentId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.DepartmentId == query.DepartmentId);
        }

        if (query.SpecialityId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.SpecialityId == query.SpecialityId);
        }

        if (query.SpecializationId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.SpecializationId == query.SpecializationId);
        }

        if (!string.IsNullOrWhiteSpace(query.OrganizationName))
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.OrganizationName != null &&
                s.Context.OrganizationName.Contains(query.OrganizationName));
        }

        List<Submission> submissions = await submissionsQuery.ToListAsync(cancellationToken);

        if (submissions.Count == 0)
        {
            return new SubmissionStatisticsResponse
            {
                FormId = query.FormId,
                TotalSubmissions = 0,
                AverageScores = [],
                ResultScores = [],
                StandardDeviations = [],
                OverallAverage = 0,
                OverallStandardDeviation = 0
            };
        }

        var numericQuestions = form.Questions
            .Where(q => q.Type == QuestionType.Number || q.Type == QuestionType.Rating)
            .OrderBy(q => q.Order)
            .ToList();

        var averageScores = new List<decimal>();
        var resultScores = new List<decimal>();
        var standardDeviations = new List<decimal>();

        foreach (Question question in numericQuestions)
        {
            var questionAnswers = submissions
                .SelectMany(s => s.Answers)
                .Where(a => a.QuestionId == question.Id && a.NumericValue.HasValue)
                .ToList();

            if (questionAnswers.Count == 0)
            {
                averageScores.Add(0);
                resultScores.Add(0);
                standardDeviations.Add(0);
                continue;
            }

            decimal average = questionAnswers.Average(a => a.NumericValue!.Value);
            averageScores.Add(average);

            decimal result = 0;
            if (question.Type == QuestionType.Number)
            {
                decimal weightedSum = questionAnswers
                    .Where(a => a.Weight.HasValue && a.Weight.Value > 0)
                    .Sum(a => a.NumericValue!.Value / a.Weight!.Value);
                int count = questionAnswers.Count(a => a.Weight.HasValue && a.Weight.Value > 0);
                result = count > 0 ? weightedSum / count * 100 : 0;
            }
            else
            {
                result = average;
            }
            resultScores.Add(result);

            decimal variance = questionAnswers
                .Select(a => a.NumericValue!.Value)
                .Select(score => (score - average) * (score - average))
                .Average();
            decimal stdDev = (decimal)Math.Sqrt((double)variance);
            standardDeviations.Add(stdDev);
        }

        decimal overallAverage = resultScores.Count > 0 ? resultScores.Average() : 0;
        decimal overallStdDev = standardDeviations.Count > 0 ? standardDeviations.Average() : 0;

        return new SubmissionStatisticsResponse
        {
            FormId = query.FormId,
            TotalSubmissions = submissions.Count,
            AverageScores = averageScores,
            ResultScores = resultScores,
            StandardDeviations = standardDeviations,
            OverallAverage = overallAverage,
            OverallStandardDeviation = overallStdDev
        };
    }
}
