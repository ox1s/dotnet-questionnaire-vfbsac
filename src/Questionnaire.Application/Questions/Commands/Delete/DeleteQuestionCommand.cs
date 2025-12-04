using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application.Questions.Commands.Delete;

public sealed record DeleteQuestionCommand(int Id) : ICommand;