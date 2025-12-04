using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Answers;

public static class AnswerErrors
{
    public static Error NotFound(int answerId) => Error.NotFound(
        "Answer.NotFound",
        $"The answer with Id = '{answerId}' was not found.");

    public static Error AlreadySubmitted(int formId, int userId) => Error.Conflict(
        "Answer.AlreadySubmitted",
        $"The user with Id = '{userId}' has already submitted an answer for form with Id = '{formId}'.");
}
