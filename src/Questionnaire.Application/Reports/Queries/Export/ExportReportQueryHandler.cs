using ErrorOr;
using MediatR;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Application.Reports.Queries.GetSummary;

namespace Questionnaire.Application.Reports.Queries.Export;

public class ExportReportQueryHandler : IRequestHandler<ExportReportQuery, ErrorOr<byte[]>>
{
    private readonly ISender _mediator;
    private readonly IReportGenerator _reportGenerator;

    public ExportReportQueryHandler(ISender mediator, IReportGenerator reportGenerator)
    {
        _mediator = mediator;
        _reportGenerator = reportGenerator;
    }

    public async Task<ErrorOr<byte[]>> Handle(ExportReportQuery request, CancellationToken cancellationToken)
    {
        var summaryQuery = new GetSummaryReportQuery(request.FormId);
        var summaryResult = await _mediator.Send(summaryQuery, cancellationToken);

        if (summaryResult.IsError)
        {
            return summaryResult.Errors;
        }

        var fileBytes = _reportGenerator.GenerateSummaryReport(summaryResult.Value);

        return fileBytes;
    }
}