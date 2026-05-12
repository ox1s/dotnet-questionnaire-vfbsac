using SharedKernel;

namespace Domain.Questionnaires.Forms;

public static class QuestionErrors
{
    public static Error EmptyField => Error.Failure(
        "Questions.EmptyField",
        $"{Resources.DomainErrors.Questions_EmptyField}");

    public static Error OrderInvalid => Error.Failure(
        "Questions.OrderInvalid",
        $"{Resources.DomainErrors.Questions_OrderInvalid}");

    public static Error NotFound(Guid id) => Error.NotFound(
        "Questions.NotFound",
        $"{Resources.DomainErrors.Question_NotFound}, Id = {id}");
}
