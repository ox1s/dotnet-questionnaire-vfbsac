
namespace Questionnaire.Application.Reports.Queries.GetSummary;

public record SummaryReportResult(
    int FormId,
    string FormName,
    int TotalSubmissions,
    List<QuestionSummaryResult> Questions);

