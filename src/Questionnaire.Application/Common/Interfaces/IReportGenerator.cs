using Questionnaire.Application.Reports.Common;

namespace Questionnaire.Application.Common.Interfaces;

public interface IReportGenerator
{
    byte[] GenerateSummaryReport(SummaryReportResponse data);
}
