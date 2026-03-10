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

    public static Error QuestionOrderExists(int order) => Error.Failure(
        "Forms.QuestionOrderExists",
        $"Question with order {order} already exists");

    public static Error AlreadyActive(Guid formId) => Error.Failure(
        "Forms.AlreadyActive",
        $"The form with the Id = '{formId}' is already active");

    public static Error AlreadyDeactivated(Guid formId) => Error.Failure(
        "Forms.AlreadyDeactivated",
        $"The form with the Id = '{formId}' is already deactivated");
}
