namespace Questionnaire.Application.Surveys.Commands.Submit;

public record AnswerDetailItem(
    int QuestionId,
    int? Weight,
    int? Mark,
    string? TextResponse);