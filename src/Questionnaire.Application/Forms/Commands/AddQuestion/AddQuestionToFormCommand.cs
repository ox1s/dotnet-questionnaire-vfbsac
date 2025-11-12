using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Forms.Commands.AddQuestion;

public record AddQuestionToFormCommand(
    int FormId,
    int QuestionId,
    int Order) : IRequest<ErrorOr<Success>>;