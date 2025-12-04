using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Reports.Queries.GetSummary;
using Questionnaire.Contracts.Reports;
using Questionnaire.Application.Reports.Queries.Export;
using Questionnaire.SharedKernel;
using Questionnaire.Api.Common;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("reports")]
[Authorize(Roles = "admin")]
public class ReportsController : ApiController
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary/{formId:int}")]
    public async Task<IActionResult> GetSummaryReport(int formId)
    {
        GetSummaryReportQuery query = new GetSummaryReportQuery(formId);
        Result<SummaryReportResponse> result = await _sender.Send(query);

        return result.Match(
            report => Ok(report),
            error => Problem(error));
    }

    [HttpGet("export/{formId:int}")]
    public async Task<IActionResult> ExportReport(int formId)
    {
        ExportReportQuery query = new ExportReportQuery(formId);
        Result<byte[]> result = await _sender.Send(query);

        return result.Match(
            fileBytes => File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Report_Form_{formId}.docx"),
            error => Problem(error));
    }
}