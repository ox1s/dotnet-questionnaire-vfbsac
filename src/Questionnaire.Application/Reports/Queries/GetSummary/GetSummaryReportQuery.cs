using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Reports.Queries.GetSummary;

public record GetSummaryReportQuery(int FormId) : IRequest<ErrorOr<SummaryReportResult>>;