using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Forms;

public static class FormErrors
{
    public static Error NotFound(int formId) => Error.NotFound(
        "Form.NotFound",
        $"The form with Id = '{formId}' was not found.");

    public static Error AlreadyExists(string name) => Error.Conflict(
        "Form.AlreadyExists",
        $"The form with name '{name}' already exists.");

    public static Error QuestionAlreadyExists(int questionId) => Error.Conflict(
        "Form.QuestionAlreadyExists",
        $"The question with Id = '{questionId}' is already in the form.");

    public static Error QuestionNotFound(int questionId) => Error.NotFound(
        "Form.QuestionNotFound",
        $"The question with Id = '{questionId}' is not found in the form.");
}
