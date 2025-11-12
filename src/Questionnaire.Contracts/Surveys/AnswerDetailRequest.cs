namespace Questionnaire.Contracts.Surveys;

public record AnswerDetailRequest(
    int QuestionId,
    int? Weight,
    int? Mark,
    string? TextResponse);