using SharedKernel;

namespace Domain.Questionnaires.FormAggregate;

public static class QuestionErrors
{
    public static Error EmptyField => Error.Failure(
        "Questions.EmptyField",
        "Text cannot be empty");

    public static Error OrderInvalid => Error.Failure(
        "Questions.OrderInvalid",
        "Order must be non-negative");
}
