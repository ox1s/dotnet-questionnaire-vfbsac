using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Forms.Commands.Create;
using Questionnaire.Contracts.Forms;
using Questionnaire.Domain.Forms;
using Questionnaire.Application.Forms.Commands.AddQuestion;
using Questionnaire.Application.Forms.Queries.GetAll;
using Questionnaire.Api.Common;
using Questionnaire.Application.Forms.Queries.GetById;
using Questionnaire.Contracts.Questions;
using Questionnaire.Application.Forms.Commands.Delete;
using Questionnaire.Application.Forms.Commands.RemoveQuestion;
using Questionnaire.SharedKernel;
using ApplicationFormResponse = Questionnaire.Application.Forms.Common.FormResponse;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("forms")]
[Authorize]
public class FormsController : ApiController
{
    private readonly ISender _sender;

    public FormsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllForms()
    {
        GetAllFormsQuery query = new GetAllFormsQuery();
        Result<IEnumerable<ApplicationFormResponse>> getFormsResult = await _sender.Send(query);

        return getFormsResult.Match(
            forms => Ok(forms.Select(ApplicationToContractMappers.ToContract)),
            error => Problem(error));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateForm(CreateFormRequest request)
    {
        CreateFormCommand command = new CreateFormCommand(request.Name);
        Result<Form> createFormResult = await _sender.Send(command);

        return createFormResult.Match(
            form => Ok(ApplicationToContractMappers.ToContract(new ApplicationFormResponse(form.Id, form.Name, form.IsActive, null))),
            error => Problem(error));
    }
    [HttpPost("{formId:int}/questions/{questionId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddQuestionToForm(
            [FromRoute] int formId,
            [FromRoute] int questionId,
            [FromBody] AddQuestionToFormRequest request)
    {
        AddQuestionToFormCommand command = new AddQuestionToFormCommand(formId, questionId, request.Order);
        Result result = await _sender.Send(command);

        return result.Match(
            () => NoContent(),
            error => Problem(error));
    }
    [HttpGet("{formId:int}")]
    public async Task<IActionResult> GetFormById(int formId)
    {
        GetFormByIdQuery query = new GetFormByIdQuery(formId);
        Result<ApplicationFormResponse> getFormResult = await _sender.Send(query);

        return getFormResult.Match(
            form => Ok(ApplicationToContractMappers.ToContract(form)),
            error => Problem(error));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteForm(int id)
    {
        DeleteFormCommand command = new DeleteFormCommand(id);
        Result result = await _sender.Send(command);

        return result.Match(
            () => NoContent(),
            error => Problem(error));
    }
    
    [HttpDelete("{formId:int}/questions/{questionId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemoveQuestionFromForm(int formId, int questionId)
    {
        RemoveQuestionFromFormCommand command = new RemoveQuestionFromFormCommand(formId, questionId);
        Result result = await _sender.Send(command);

        return result.Match(
            () => NoContent(),
            error => Problem(error));
    }





}