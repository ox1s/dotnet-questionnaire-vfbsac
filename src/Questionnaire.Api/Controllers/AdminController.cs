using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Questions.Commands.Create;
using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Entities;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Questionnaire.Application.Questions.Queries.GetAll;
using Questionnaire.Application.Questions.Commands.Delete;
using Questionnaire.SharedKernel;

using Questionnaire.Api.Common;

namespace Questionnaire.Api.Controllers;

[Route("admin")]
[Authorize(Roles = "admin")]
public class AdminController : ApiController
{
    private readonly ISender _sender;

    public AdminController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("data")]
    public IActionResult GetAdminData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userLogin = User.FindFirstValue(JwtRegisteredClaimNames.Name);

        return Ok(new { Message = $"Hello, admin {userLogin}! Your ID is {userId}. This is secret data." });
    }

    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(CreateQuestionRequest request)
    {
        if (!Enum.IsDefined(typeof(Contracts.Questions.QuestionType), request.Type))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid question type.");
        }

        CreateQuestionCommand command = new CreateQuestionCommand(
            request.Text,
            request.Type,
            request.Options);

        Result<QuestionResponse> createQuestionResult = await _sender.Send(command);

        return createQuestionResult.Match(
            question => Ok(question),
            error => Problem(error));
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetAllQuestions()
    {
        GetAllQuestionsQuery query = new GetAllQuestionsQuery();
        Result<IEnumerable<QuestionResponse>> getQuestionsResult = await _sender.Send(query);

        return getQuestionsResult.Match(
            questions => Ok(questions),
            error => Problem(error));
    }
    
    [HttpDelete("questions/{id:int}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        DeleteQuestionCommand command = new DeleteQuestionCommand(id);
        Result result = await _sender.Send(command);

        return result.Match(
            () => NoContent(),
            error => Problem(error));
    }
}