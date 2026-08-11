namespace Application.Reports.Queries.Shared;

/// <summary>
/// Consumer satisfaction rating scale from "Методика оценки удовлетворенности потребителей"
/// (Table 1): thresholds are on <see cref="QuestionStatistics.SatisfactionPercentage"/>.
/// </summary>
public enum SatisfactionRating
{
    /// <summary>УП &lt; 40%.</summary>
    Unsatisfactory = 0,

    /// <summary>40% &le; УП &lt; 60%.</summary>
    Satisfactory = 1,

    /// <summary>60% &le; УП &lt; 80%.</summary>
    Good = 2,

    /// <summary>УП &ge; 80%.</summary>
    Excellent = 3
}
