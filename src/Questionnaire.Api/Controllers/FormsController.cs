using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Forms.Commands.Create;
using Questionnaire.Contracts.Forms;
using Questionnaire.Domain.Entities;
using Questionnaire.Application.Forms.Commands.AddQuestion;
using Questionnaire.Application.Forms.Queries.GetAll;
using Questionnaire.Api.Common;
using Questionnaire.Application.Forms.Queries.GetById;
using Questionnaire.Contracts.Questions;
using Questionnaire.Application.Forms.Commands.Delete;
using Questionnaire.Application.Forms.Commands.RemoveQuestion;

namespace Questionnaire.Api.Controllers;

[ApiController]
[Route("forms")]
[Authorize]
public class FormsController : ApiController
{
    private readonly ISender _mediator;


    public FormsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllForms()
    {
        var query = new GetAllFormsQuery();
        var getFormsResult = await _mediator.Send(query);

        return getFormsResult.Match(
            forms => Ok(forms.Select(MapToFormResponse)),
            errors => Problem(errors));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateForm(CreateFormRequest request)
    {
        var command = new CreateFormCommand(request.Name);
        var createFormResult = await _mediator.Send(command);

        return createFormResult.Match(
            form => Ok(MapToFormResponse(form)),
            errors => Problem(errors));
    }
    [HttpPost("{formId:int}/questions/{questionId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddQuestionToForm(
            [FromRoute] int formId,
            [FromRoute] int questionId,
            [FromBody] AddQuestionToFormRequest request)
    {
        var command = new AddQuestionToFormCommand(formId, questionId, request.Order);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
    [HttpGet("{formId:int}")]
    public async Task<IActionResult> GetFormById(int formId)
    {
        var query = new GetFormByIdQuery(formId);
        var getFormResult = await _mediator.Send(query);

        return getFormResult.Match(
            form => Ok(MapToDetailedFormResponse(form)),
            errors => Problem(errors));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteForm(int id)
    {
        var command = new DeleteFormCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }
    
    [HttpDelete("{formId:int}/questions/{questionId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemoveQuestionFromForm(int formId, int questionId)
    {
        var command = new RemoveQuestionFromFormCommand(formId, questionId);
        var result = await _mediator.Send(command);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors));
    }


    private static FormResponse MapToFormResponse(Form form)
    {
        return new FormResponse(form.Id, form.Name, form.IsActive, null);
    }


    private static FormResponse MapToDetailedFormResponse(Form form)
    {
        return new FormResponse(
            form.Id,
            form.Name,
            form.IsActive,
            form.FormQuestions.Select(fq => ApiMappers.ToDto(fq.Question)).ToList()
        );
    }



}