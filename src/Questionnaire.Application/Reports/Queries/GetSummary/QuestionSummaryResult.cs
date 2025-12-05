using Questionnaire.Domain.Questions;

namespace Questionnaire.Application.Reports.Queries.GetSummary;

public record QuestionSummaryResult(
    int QuestionId,
    string QuestionText,
    QuestionType QuestionType,
    ICollection<QuestionOption> Options,

    double AverageMark,
    double AverageWeight,
    int RatingResponseCount,
    List<string> TextResponses,
    Dictionary<int, int> ChoiceCounts 
);