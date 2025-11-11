namespace Questionnaire.Contracts.Questions;

/// <summary>
/// Defines the type of a question for the API contract.
/// </summary>
public enum QuestionType
{
    Rating, // Оценка с весом
    Text,   // Текстовый ответ
    Choice  // Выбор одного или нескольких вариантов 
}