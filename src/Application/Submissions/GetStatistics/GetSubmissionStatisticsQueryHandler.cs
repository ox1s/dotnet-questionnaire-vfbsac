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

            // Логика для нового типа WeightedRating
            if (question.Type == QuestionType.WeightedRating)
            {
                // Берем ответы, где есть и оценка, и вес
                var validAnswers = questionAnswers
                    .Where(a => a.NumericValue.HasValue && a.Weight.HasValue && a.Weight.Value > 0)
                    .ToList();

                if (validAnswers.Count > 0)
                {
                    // Вариант расчета: Средний процент удовлетворенности
                    // (Оценка / Вес) * 10. 
                    // Пример: Оценка 8, Вес 10 -> 8 баллов.
                    // Пример: Оценка 4, Вес 5 -> (4/5)*10 = 8 баллов.

                    decimal sumOfNormalizedScores = validAnswers
                        .Sum(a => a.NumericValue!.Value / a.Weight!.Value * 10);

                    result = sumOfNormalizedScores / validAnswers.Count;
                }
            }
            else if (question.Type == QuestionType.Number)
            {
                // Старая логика для простых чисел (если там использовался вес как коэффициент значимости вопроса)
                // Если веса нет, просто среднее
                result = average;
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
