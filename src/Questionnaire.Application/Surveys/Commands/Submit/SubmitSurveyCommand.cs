using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application.Surveys.Commands.Submit;

public sealed record SubmitSurveyCommand(
    int FormId,
    List<AnswerDetailItem> Details) : ICommand;

