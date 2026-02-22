using Application.Submissions.GetStatistics;

namespace Application.Abstractions.Reports;

public interface IReportGenerator
{
    byte[] GenerateFormReport(string formTitle, SubmissionStatisticsResponse stats);
}
