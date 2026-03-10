using SharedKernel;

namespace Domain.Questionnaires.SubmissionAggregate;

public static class AnswerErrors
{
    public static Error NotFound(Guid answerId) =>
        Error.NotFound(
            "Answers.NotFound",
            $"The answer with the Id = '{answerId}' was not found");
    public static Error ValueRequired =>
        Error.Failure(
            "Answers.ValueRequired",
            "Either Value or NumericValue must be provided");
    public static Error InvalidScore(int MinValue, int MaxValue) =>
        Error.Failure(
            "Answers.InvalidScore",
            $"Score must be between {MinValue} and {MaxValue}");
    public static Error InvalidWeight(int MinValue, int MaxValue) =>
        Error.Failure(
            "Answers.InvalidWeight",
            $"Weight must be between {MinValue} and {MaxValue}");
}
