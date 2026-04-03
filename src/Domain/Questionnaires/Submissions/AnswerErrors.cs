using SharedKernel;

namespace Domain.Questionnaires.Submissions;

public static class AnswerErrors
{
    public static Error NotFound(Guid answerId) =>
        Error.NotFound(
            "Answers.NotFound",
            $"{Resources.DomainErrors.Answers_NotFound}, Id = '{answerId}'");
    public static Error ValueRequired =>
        Error.Failure(
            "Answers.ValueRequired",
            $"{Resources.DomainErrors.Answers_ValueRequired}");
    public static Error InvalidScore(int MinValue, int MaxValue) =>
        Error.Failure(
            "Answers.InvalidScore",
            $"{Resources.DomainErrors.Answers_InvalidScore}, Min = '{MinValue}', Max = '{MaxValue}'");
    public static Error InvalidWeight(int MinValue, int MaxValue) =>
        Error.Failure(
            "Answers.InvalidWeight",
            $"{Resources.DomainErrors.Answers_InvalidWeight}, Min = '{MinValue}', Max = '{MaxValue}'");
}
