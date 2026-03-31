using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Submissions.GetStatistics;

internal sealed class GetSubmissionStatisticsQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetSubmissionStatisticsQuery, SubmissionStatisticsResponse>
{
    // TODO: Выколите глаза. Сервис в домене. Оптимизация
    public async Task<Result<SubmissionStatisticsResponse>> Handle(
        GetSubmissionStatisticsQuery query,
        CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .AsNoTracking()
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f => f.Id == query.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure<SubmissionStatisticsResponse>(
                FormErrors.NotFound(query.FormId));
        }

        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s => s.FormId == query.FormId);

        if (query.DisciplineId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.DisciplineId == query.DisciplineId);
        }
        if (query.TeacherId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.TeacherId == query.TeacherId);
        }
        if (query.DepartmentId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.DepartmentId == query.DepartmentId);
        }
        if (query.SpecialityId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.SpecialityId == query.SpecialityId);
        }
        if (query.SpecializationId.HasValue)
        {
            submissionsQuery = submissionsQuery
                .Where(s => s.Context.SpecializationId == query.SpecializationId);
        }
        if (!string.IsNullOrWhiteSpace(query.OrganizationName))
        {
            submissionsQuery = submissionsQuery
                .Where(s =>
                    s.Context.OrganizationName != null
                    && s.Context.OrganizationName.Contains(query.OrganizationName));
        }

        int totalSubmissions = await submissionsQuery.CountAsync(cancellationToken);

        if (totalSubmissions == 0)
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

        var answersData = await submissionsQuery
            .SelectMany(s => s.Answers)
            .Where(a => a.NumericValue != null) // Исключаем текстовые ответы
            .Select(a => new
            {
                a.QuestionId,
                // Явное получение значения, т.к. фильтр выше гарантирует !null
                NumericValue = a.NumericValue!.Value,
                a.Weight
            })
            .ToListAsync(cancellationToken);

        // 6. Группировка данных в памяти
        var groupedAnswers = answersData
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var numericQuestions = form.Questions
            .Where(q => q.Type == QuestionType.Number ||
                        q.Type == QuestionType.Rating ||
                        q.Type == QuestionType.WeightedRating)
            .OrderBy(q => q.Order)
            .ToList();

        var averageScores = new List<decimal>();
        var resultScores = new List<decimal>();
        var standardDeviations = new List<decimal>();

        foreach (Question question in numericQuestions)
        {
            // Если ответов нет, добавляем нули
            if (!groupedAnswers.TryGetValue(question.Id, out var questionAnswers) || questionAnswers.Count == 0)
            {
                averageScores.Add(0);
                resultScores.Add(0);
                standardDeviations.Add(0);
                continue;
            }

            // --- Расчет среднего арифметического ---
            decimal average = questionAnswers.Average(a => a.NumericValue);
            averageScores.Add(average);

            // --- Расчет итогового балла (с учетом весов) ---
            decimal result = 0;

            if (question.Type == QuestionType.WeightedRating)
            {
                // Для вопросов с весом: нормализуем оценку относительно веса к 10-балльной шкале.
                // Формула: (Оценка / Вес) * 10
                // Пример: Оценка 4 из 5 (вес) -> (4/5)*10 = 8 баллов.
                var validWeightedAnswers = questionAnswers
                    .Where(a => a.Weight.HasValue && a.Weight.Value > 0)
                    .ToList();

                if (validWeightedAnswers.Count > 0)
                {
                    decimal sumOfNormalizedScores = validWeightedAnswers
                        .Sum(a => a.NumericValue / a.Weight!.Value * 10);
                    result = sumOfNormalizedScores / validWeightedAnswers.Count;
                }
            }
            else
            {
                // Для обычных вопросов результат равен среднему
                result = average;
            }

            resultScores.Add(result);

            // --- Расчет стандартного отклонения ---
            // Variance = Average((x - mean)^2)
            decimal variance = questionAnswers
                .Average(a => (a.NumericValue - average) * (a.NumericValue - average));
            decimal stdDev = (decimal)Math.Sqrt((double)variance);
            standardDeviations.Add(stdDev);
        }

        // 7. Расчет общих показателей по всей анкете
        decimal overallAverage = resultScores.Count > 0 ? resultScores.Average() : 0;
        decimal overallStdDev = standardDeviations.Count > 0 ? standardDeviations.Average() : 0;

        return new SubmissionStatisticsResponse
        {
            FormId = query.FormId,
            TotalSubmissions = totalSubmissions,
            AverageScores = averageScores,
            ResultScores = resultScores,
            StandardDeviations = standardDeviations,
            OverallAverage = overallAverage,
            OverallStandardDeviation = overallStdDev
        };
    }
}
