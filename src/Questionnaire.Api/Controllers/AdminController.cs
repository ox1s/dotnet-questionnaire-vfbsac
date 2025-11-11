using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Questionnaire.Application.Questions.Commands.Create;
using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Entities;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Questionnaire.Application.Questions.Queries.GetAll;

using ContractsQuestionType = Questionnaire.Contracts.Questions.QuestionType;
using DomainQuestionType = Questionnaire.Domain.Entities.QuestionType;

namespace Questionnaire.Api.Controllers;

[Route("admin")]
[Authorize(Roles = "admin")]
public class AdminController : ApiController
{
    private readonly ISender _mediator;

    public AdminController(ISender mediator)
    {
        _mediator = mediator;
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
        if (!Enum.IsDefined(typeof(ContractsQuestionType), request.Type))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid question type.");
        }

        var domainType = ToDomain(request.Type);

        var command = new CreateQuestionCommand(
            request.Text,
            domainType,
            request.Options);

        var createQuestionResult = await _mediator.Send(command);

        return createQuestionResult.Match(
            question => Ok(ToDto(question)),
            errors => Problem(errors));
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetAllQuestions()
    {
        var query = new GetAllQuestionsQuery();
        var getQuestionsResult = await _mediator.Send(query);

        return getQuestionsResult.Match(
            questions => Ok(questions.Select(ToDto)), 
            errors => Problem(errors));
    }

    private static QuestionResponse ToDto(Question question)
    {
        return new QuestionResponse(
            question.Id,
            question.Text,
            ToDto(question.Type),
            question.Options.Select(o => new OptionResponse(o.Id, o.Text)).ToList()
        );
    }

    private static ContractsQuestionType ToDto(DomainQuestionType domainType)
    {
        return domainType switch
        {
            DomainQuestionType.Rating => ContractsQuestionType.Rating,
            DomainQuestionType.Text => ContractsQuestionType.Text,
            DomainQuestionType.Choice => ContractsQuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map domain question type to contract."),
        };
    }

    private static DomainQuestionType ToDomain(ContractsQuestionType contractType)
    {
        return contractType switch
        {
            ContractsQuestionType.Rating => DomainQuestionType.Rating,
            ContractsQuestionType.Text => DomainQuestionType.Text,
            ContractsQuestionType.Choice => DomainQuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map contract question type to domain."),
        };
    }
}