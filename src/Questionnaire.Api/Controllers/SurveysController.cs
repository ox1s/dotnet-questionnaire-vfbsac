using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Api.Common;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Surveys.Commands.Submit;
using Questionnaire.Application.Surveys.Queries.GetAvailable;
using Questionnaire.Contracts.Forms;
using Questionnaire.Contracts.Surveys;
using Questionnaire.SharedKernel;
using ApplicationFormResponse = Questionnaire.Application.Forms.Common.FormResponse;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("surveys")]
[Authorize]
public class SurveysController : ApiController
{
    private readonly ISender _sender;

    public SurveysController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableSurveys()
    {
        GetAvailableSurveysQuery query = new GetAvailableSurveysQuery();
        Result<IEnumerable<ApplicationFormResponse>> result = await _sender.Send(query);

        return result.Match(
            forms => Ok(forms.Select(ApplicationToContractMappers.ToContract)),
            error => Problem(error));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitSurvey(SubmitSurveyRequest request)
    {
        SubmitSurveyCommand command = new SubmitSurveyCommand(
            request.FormId,
            request.Details
                .Select(d => new AnswerDetailItem(
                    d.QuestionId,
                    d.Weight,
                    d.Mark,
                    d.TextResponse))
                .ToList());

        Result result = await _sender.Send(command);

        return result.Match(
            () => Ok(),
            error => Problem(error));
    }
}