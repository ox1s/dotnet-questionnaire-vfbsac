namespace Application.Reports.Queries.GetComparative;

public sealed record ComparativeReportResponse(
    string QuestionText,
    double PeriodA_Average,
    double PeriodB_Average,
    double Delta
);
