using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application.Forms.Commands.AddQuestion;

public sealed record AddQuestionToFormCommand(
    int FormId,
    int QuestionId,
    int Order) : ICommand;