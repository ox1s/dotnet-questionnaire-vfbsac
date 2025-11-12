namespace Questionnaire.Contracts.Surveys;

public record SubmitSurveyRequest(
    int FormId,
    List<AnswerDetailRequest> Details);

