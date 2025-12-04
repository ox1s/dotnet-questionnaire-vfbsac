using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application.Forms.Commands.RemoveQuestion;

public sealed record RemoveQuestionFromFormCommand(int FormId, int QuestionId) : ICommand;