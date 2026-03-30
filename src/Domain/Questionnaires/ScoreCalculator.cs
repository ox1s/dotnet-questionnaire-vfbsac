using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;

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

        // Для обычных вопросов (Number, Rating) считаем простое среднее арифметическое
        if (type != QuestionType.WeightedRating)
        {
            return validAnswers.Average(a => a.NumericValue!.Value);
        }

        // Для вопросов с весом
        var weightedAnswers = validAnswers
            .Where(a => a.Weight is > 0)
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
}
