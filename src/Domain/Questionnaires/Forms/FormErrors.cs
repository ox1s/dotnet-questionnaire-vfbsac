using SharedKernel;

namespace Domain.Questionnaires.Forms;

public static class FormErrors
{
    public static Error NotFound(Guid formId) => Error.NotFound(
        "Forms.NotFound",
        $"{Resources.DomainErrors.Forms_NotFound}, Id = '{formId}'");

    public static Error QuestionNotFound(Guid questionId) => Error.NotFound(
        "Forms.QuestionNotFound",
        $"{Resources.DomainErrors.Forms_QuestionNotFound}, Id = '{questionId}'");

    public static Error FormInactive(Guid formId) => Error.Failure(
        "Forms.Inactive",
        $"{Resources.DomainErrors.Forms_Inactive}, Id = '{formId}'");

    public static Error QuestionOrderExists(int order) => Error.Failure(
        "Forms.QuestionOrderExists",
        $"{Resources.DomainErrors.Forms_QuestionOrderExists}, Order = '{order}'");

    public static Error AlreadyActive(Guid formId) => Error.Failure(
        "Forms.AlreadyActive",
        $"{Resources.DomainErrors.Forms_AlreadyActive}, Id = '{formId}'");

    public static Error AlreadyDeactivated(Guid formId) => Error.Failure(
        "Forms.AlreadyDeactivated",
        $"{Resources.DomainErrors.Forms_AlreadyDeactivated}, Id = '{formId}'");
}
