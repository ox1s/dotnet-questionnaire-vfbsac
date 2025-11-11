namespace Questionnaire.Contracts.Questions;

public record QuestionResponse(
    int Id,
    string Text,
    QuestionType Type,
    List<OptionResponse> Options);

