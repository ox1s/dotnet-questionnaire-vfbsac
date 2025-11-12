using Questionnaire.Application.Reports.Queries.GetSummary;

namespace Questionnaire.Application.Common.Interfaces;

public interface IReportGenerator
{
    byte[] GenerateSummaryReport(SummaryReportResult data);
}
