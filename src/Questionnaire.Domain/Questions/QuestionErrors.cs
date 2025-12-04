using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Questions;

public static class QuestionErrors
{
    public static Error NotFound(int questionId) => Error.NotFound(
        "Question.NotFound",
        $"The question with Id = '{questionId}' was not found.");

    public static Error AlreadyExists(string text) => Error.Conflict(
        "Question.AlreadyExists",
        $"The question with text '{text}' already exists.");
}
