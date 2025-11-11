namespace Questionnaire.Contracts.Questions;

public record CreateQuestionRequest(
    string Text,
    QuestionType Type,
    List<string>? Options = null);