using Domain.Questionnaires.FormAggregate;
using Domain.Questionnaires.SubmissionAggregate;

namespace Domain.Questionnaires;

public static class ScoreCalculator
{
    public static decimal CalculateAverage(IEnumerable<Answer> answers, QuestionType type)
    {
        var validAnswers = answers
            .Where(a => a.NumericValue.HasValue)
            .ToList();

        if (validAnswers.Count == 0)
        {
            return 0;
        }

        if (type == QuestionType.WeightedRating)
        {
            var weightedAnswers = validAnswers
                .Where(a => a.Weight.HasValue && a.Weight.Value > 0)
                .ToList();

            if (weightedAnswers.Count == 0)
            {
                return 0;
            }

            // Формула: Приведение к 10-балльной шкале относительно веса
            // (Оценка / Вес) * 10
            // Пример 1: Оценка 8, Вес 10 -> (8/10)*10 = 8.0
            // Пример 2: Оценка 4, Вес 5  -> (4/5)*10  = 8.0 
            decimal sumOfNormalizedScores = weightedAnswers
                .Sum(a => a.NumericValue!.Value / a.Weight!.Value * 10);

            return sumOfNormalizedScores / weightedAnswers.Count;
        }

        // Для обычных вопросов (Number, Rating) считаем простое среднее арифметическое
        return validAnswers.Average(a => a.NumericValue!.Value);
    }
}
