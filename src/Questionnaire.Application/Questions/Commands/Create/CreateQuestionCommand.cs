using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Questions.Common;
using Questionnaire.Domain.Questions;

namespace Questionnaire.Application.Questions.Commands.Create;

public sealed record CreateQuestionCommand(
    string Text,
    QuestionType Type,
    List<string>? Options) : ICommand<QuestionResponse>;