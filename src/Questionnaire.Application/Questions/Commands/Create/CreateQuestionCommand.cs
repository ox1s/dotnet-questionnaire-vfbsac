using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Questions;

namespace Questionnaire.Application.Questions.Commands.Create;

public sealed record CreateQuestionCommand(
    string Text,
    Contracts.Questions.QuestionType Type,
    List<string>? Options) : ICommand<QuestionResponse>;