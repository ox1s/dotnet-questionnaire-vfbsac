using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Reports;

namespace Questionnaire.Application.Reports.Queries.GetSummary;

public sealed record GetSummaryReportQuery(int FormId) : IQuery<SummaryReportResponse>;