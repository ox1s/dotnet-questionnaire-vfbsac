using SharedKernel;

namespace Domain.Questionnaires.FormAggregate;

public static class FormErrors
{
    public static Error NotFound(Guid formId) => Error.NotFound(
        "Forms.NotFound",
        $"The form with the Id = '{formId}' was not found");

    public static Error QuestionNotFound(Guid questionId) => Error.NotFound(
        "Forms.QuestionNotFound",
        $"The question with the Id = '{questionId}' was not found");

    public static Error FormInactive(Guid formId) => Error.Failure(
        "Forms.Inactive",
        $"The form with the Id = '{formId}' is not active");
}
