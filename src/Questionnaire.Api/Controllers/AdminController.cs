using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Questions.Commands.Create;
using Questionnaire.Contracts.Questions;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Questionnaire.Application.Questions.Queries.GetAll;
using Questionnaire.Application.Questions.Commands.Delete;
using Questionnaire.SharedKernel;
using Questionnaire.Api.Common;
using ApplicationQuestionResponse = Questionnaire.Application.Questions.Common.QuestionResponse;

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

        var domainType = MapToDomainQuestionType(request.Type);
        CreateQuestionCommand command = new CreateQuestionCommand(
            request.Text,
            domainType,
            request.Options);

        Result<ApplicationQuestionResponse> createQuestionResult = await _sender.Send(command);

        return createQuestionResult.Match(
            question => Ok(ApplicationToContractMappers.ToContract(question)),
            error => Problem(error));
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetAllQuestions()
    {
        GetAllQuestionsQuery query = new GetAllQuestionsQuery();
        Result<IEnumerable<ApplicationQuestionResponse>> getQuestionsResult = await _sender.Send(query);

        return getQuestionsResult.Match(
            questions => Ok(questions.Select(ApplicationToContractMappers.ToContract)),
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

    private static Domain.Questions.QuestionType MapToDomainQuestionType(Contracts.Questions.QuestionType contractType)
    {
        return contractType switch
        {
            Contracts.Questions.QuestionType.Rating => Domain.Questions.QuestionType.Rating,
            Contracts.Questions.QuestionType.Text => Domain.Questions.QuestionType.Text,
            Contracts.Questions.QuestionType.Choice => Domain.Questions.QuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map contract question type to domain."),
        };
    }
}