using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Forms.Commands.RemoveQuestion;

public record RemoveQuestionFromFormCommand(int FormId, int QuestionId) : IRequest<ErrorOr<Success>>;