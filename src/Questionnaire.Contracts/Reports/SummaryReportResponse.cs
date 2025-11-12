using Questionnaire.Contracts.Questions;

namespace Questionnaire.Contracts.Reports;

public record SummaryReportResponse(
    int FormId,
    string FormName,
    int TotalSubmissions,
    List<QuestionSummaryResponse> Questions);

public record QuestionSummaryResponse(
    int QuestionId,
    string QuestionText,
    QuestionType QuestionType,
    RatingSummaryData? RatingData,
    List<string>? TextData,
    List<ChoiceSummaryData>? ChoiceData);

public record RatingSummaryData(double AverageMark, double AverageWeight, int ResponseCount);
public record ChoiceSummaryData(int OptionId, string OptionText, int SelectedCount);