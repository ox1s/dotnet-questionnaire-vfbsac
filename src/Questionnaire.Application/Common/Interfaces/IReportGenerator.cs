using Questionnaire.Contracts.Reports;

namespace Questionnaire.Application.Common.Interfaces;

public interface IReportGenerator
{
    byte[] GenerateSummaryReport(SummaryReportResponse data);
}
