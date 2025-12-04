using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Application.Reports.Queries.GetSummary;
using Questionnaire.Contracts.Reports;
using Questionnaire.Domain.Forms;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Reports.Queries.Export;

internal sealed class ExportReportQueryHandler : IQueryHandler<ExportReportQuery, byte[]>
{
    private readonly ISender _sender;
    private readonly IReportGenerator _reportGenerator;

    public ExportReportQueryHandler(ISender sender, IReportGenerator reportGenerator)
    {
        _sender = sender;
        _reportGenerator = reportGenerator;
    }

    public async Task<Result<byte[]>> Handle(ExportReportQuery query, CancellationToken cancellationToken)
    {
        var summaryQuery = new GetSummaryReportQuery(query.FormId);
        Result<SummaryReportResponse> summaryResult = await _sender.Send(summaryQuery, cancellationToken);

        if (summaryResult.IsFailure)
        {
            return Result.Failure<byte[]>(summaryResult.Error);
        }

        byte[] fileBytes = _reportGenerator.GenerateSummaryReport(summaryResult.Value);

        return Result.Success(fileBytes);
    }
}