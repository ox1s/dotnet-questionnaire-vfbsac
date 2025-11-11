using ErrorOr;
using MediatR;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Questions.Commands.Create;

public record CreateQuestionCommand(
    string Text,
    QuestionType Type,
    List<string>? Options) : IRequest<ErrorOr<Question>>;