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
    
    public static Error InvalidScore(int minValue, int maxValue) =>
        Error.Failure(
            "Answers.InvalidScore",
            $"{Resources.DomainErrors.Answers_InvalidScore}, Min = '{minValue}', Max = '{maxValue}'");
    
    public static Error InvalidWeight(int minValue, int maxValue) =>
        Error.Failure(
            "Answers.InvalidWeight",
            $"{Resources.DomainErrors.Answers_InvalidWeight}, Min = '{minValue}', Max = '{maxValue}'");

    public static Error InvalidTypeForText =>
        Error.Failure(
            "Answers.InvalidTypeForText",
            "Text question requires Value field only");

    public static Error InvalidTypeForNumber =>
        Error.Failure(
            "Answers.InvalidTypeForNumber",
            "Number question requires NumericValue field only");

    public static Error InvalidTypeForWeightedRating =>
        Error.Failure(
            "Answers.InvalidTypeForWeightedRating",
            "WeightedRating question requires NumericValue and Weight fields");

    public static Error InvalidTypeForMultipleChoice =>
        Error.Failure(
            "Answers.InvalidTypeForMultipleChoice",
            "MultipleChoice question requires Value field only");

    public static Error InvalidTypeForSingleChoice =>
        Error.Failure(
            "Answers.InvalidTypeForSingleChoice",
            "SingleChoice question requires Value field only");

    public static Error UnknownQuestionType =>
        Error.Failure(
            "Answers.UnknownQuestionType",
            "Unknown question type");
}
