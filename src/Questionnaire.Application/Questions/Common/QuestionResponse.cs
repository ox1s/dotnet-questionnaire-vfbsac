namespace Questionnaire.Application.Questions.Common;

public record QuestionResponse(
    int Id,
    string Text,
    Domain.Questions.QuestionType Type,
    List<OptionResponse> Options);
