using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Reports.Queries.GetSummary;
using Questionnaire.Contracts.Reports;
using ContractsQuestionType = Questionnaire.Contracts.Questions.QuestionType;
using DomainQuestionType = Questionnaire.Domain.Entities.QuestionType;
using Questionnaire.Application.Reports.Queries.Export;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("reports")]
[Authorize(Roles = "admin")]
public class ReportsController : ApiController
{
    private readonly ISender _mediator;

    public ReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary/{formId:int}")]
    public async Task<IActionResult> GetSummaryReport(int formId)
    {
        var query = new GetSummaryReportQuery(formId);
        var result = await _mediator.Send(query);

        return result.Match(
            report => Ok(MapToResponse(report)),
            errors => Problem(errors));
    }

    [HttpGet("export/{formId:int}")]
    public async Task<IActionResult> ExportReport(int formId)
    {
        var query = new ExportReportQuery(formId);
        var result = await _mediator.Send(query);

        return result.Match(
            fileBytes => File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Report_Form_{formId}.docx"),
            errors => Problem(errors));
    }
    private static SummaryReportResponse MapToResponse(SummaryReportResult result)
    {
        return new SummaryReportResponse(
            result.FormId,
            result.FormName,
            result.TotalSubmissions,
            result.Questions.Select(q => new QuestionSummaryResponse(
                q.QuestionId,
                q.QuestionText,
                MapQuestionType(q.QuestionType),
                q.QuestionType == DomainQuestionType.Rating ? new RatingSummaryData(q.AverageMark, q.AverageWeight, q.RatingResponseCount) : null,
                q.QuestionType == DomainQuestionType.Text ? q.TextResponses : null,
                q.QuestionType == DomainQuestionType.Choice
                    ? q.Options.Select(opt => new ChoiceSummaryData(
                        opt.Id,
                        opt.Text,
                        q.ChoiceCounts.TryGetValue(opt.Id, out var count) ? count : 0))
                      .ToList()
                    : null
            )).ToList()
        );
    }

    private static ContractsQuestionType MapQuestionType(DomainQuestionType domainType)
    {
        return domainType switch
        {
            DomainQuestionType.Rating => ContractsQuestionType.Rating,
            DomainQuestionType.Text => ContractsQuestionType.Text,
            DomainQuestionType.Choice => ContractsQuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map domain question type to contract."),
        };
    }
}