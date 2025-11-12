using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Questions.Commands.Delete;

public record DeleteQuestionCommand(int Id) : IRequest<ErrorOr<Success>>;