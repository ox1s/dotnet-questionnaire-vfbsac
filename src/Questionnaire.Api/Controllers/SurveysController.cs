using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Api.Common;
using Questionnaire.Application.Surveys.Commands.Submit;
using Questionnaire.Application.Surveys.Queries.GetAvailable;
using Questionnaire.Contracts.Forms;
using Questionnaire.Contracts.Surveys;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("surveys")]
[Authorize]
public class SurveysController : ApiController
{
    private readonly ISender _mediator;

    public SurveysController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableSurveys()
    {
        var query = new GetAvailableSurveysQuery();
        var result = await _mediator.Send(query);

        return result.Match(
            forms => Ok(forms.Select(form => new FormResponse(form.Id, form.Name, form.IsActive, null))),
            errors => Problem(errors));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitSurvey(SubmitSurveyRequest request)
    {
        var command = new SubmitSurveyCommand(
            request.FormId,
            request.Details
                .Select(d => new AnswerDetailItem(
                    d.QuestionId,
                    d.Weight,
                    d.Mark,
                    d.TextResponse))
                .ToList());

        var result = await _mediator.Send(command);

        return result.Match(
            _ => Ok(),
            errors => Problem(errors));
    }
}