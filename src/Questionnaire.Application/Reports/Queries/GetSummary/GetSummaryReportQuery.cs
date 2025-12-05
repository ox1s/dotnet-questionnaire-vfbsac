using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Reports.Common;

namespace Questionnaire.Application.Reports.Queries.GetSummary;

public sealed record GetSummaryReportQuery(int FormId) : IQuery<SummaryReportResponse>;