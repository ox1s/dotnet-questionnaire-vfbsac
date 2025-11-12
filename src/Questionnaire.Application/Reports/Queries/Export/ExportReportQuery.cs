using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Reports.Queries.Export;

public record ExportReportQuery(int FormId) : IRequest<ErrorOr<byte[]>>;