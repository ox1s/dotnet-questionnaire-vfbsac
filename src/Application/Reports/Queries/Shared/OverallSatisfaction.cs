namespace Application.Reports.Queries.Shared;

/// <summary>
/// Formula (6) from "Методика оценки удовлетворенности потребителей" (college-specs/Методика_оценки.md,
/// section 5.6): the overall form/blank (анкета) satisfaction, УП = mean(УП_j across all questions) ± σ̄,
/// where σ̄ (<see cref="AverageStandardDeviation"/>) is formula (5), the mean of the per-question standard
/// deviations across all n criteria of the form.
/// </summary>
/// <param name="HasData">
/// False when there were no submissions/questions to aggregate. When false, <see cref="MeanPercentage"/>,
/// <see cref="AverageStandardDeviation"/> and <see cref="Rating"/> carry meaningless default values and
/// callers must render a "no data" state instead of showing them.
/// </param>
public sealed record OverallSatisfaction(
    decimal MeanPercentage,
    decimal AverageStandardDeviation,
    SatisfactionRating Rating,
    bool HasData);
