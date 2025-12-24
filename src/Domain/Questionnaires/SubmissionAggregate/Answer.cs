using SharedKernel;

namespace Domain.Questionnaires.SubmissionAggregate;

public sealed class Answer : Entity
{
    public Guid SubmissionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string? Value { get; private set; }
    public decimal? NumericValue { get; private set; }
    public decimal? Weight { get; private set; }

    private Answer() { }

    private Answer(Guid id, Guid submissionId, Guid questionId, string? value, decimal? numericValue, decimal? weight) : base(id)
    {
        SubmissionId = submissionId;
        QuestionId = questionId;
        Value = value;
        NumericValue = numericValue;
        Weight = weight;
    }

    public static Result<Answer> Create(Guid submissionId, Guid questionId, string? value = null, decimal? numericValue = null, decimal? weight = null)
    {
        if (value is null && numericValue is null)
        {
            return Result.Failure<Answer>(Error.Failure(
                "Answers.ValueRequired",
                "Either Value or NumericValue must be provided"));
        }

        const int MinValue = 1;
        const int MaxValue = 10;

        if (numericValue.HasValue && (numericValue.Value < MinValue || numericValue.Value > MaxValue))
        {
            return Result.Failure<Answer>(Error.Validation("Answers.InvalidScore", $"Score must be between {MinValue} and {MaxValue}"));
        }

        if (weight.HasValue && (weight.Value < MinValue || weight.Value > MaxValue))
        {
            return Result.Failure<Answer>(Error.Validation("Answers.InvalidWeight", $"Weight must be between {MinValue} and {MaxValue}"));
        }
        // ---------------------------------------

        if (weight.HasValue && numericValue.HasValue && numericValue.Value > weight.Value)
        {
            return Result.Failure<Answer>(SubmissionErrors.InvalidWeight(questionId));
        }

        return new Answer(Guid.NewGuid(), submissionId, questionId, value?.Trim(), numericValue, weight);

    }
    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
    }

    public Result UpdateNumericStats(decimal? numericValue, decimal? weight)
    {
        if (weight.HasValue && numericValue.HasValue && numericValue.Value > weight.Value)
        {
            return Result.Failure(SubmissionErrors.InvalidWeight(QuestionId));
        }

        NumericValue = numericValue;
        Weight = weight;

        return Result.Success();
    }

    public void UpdateWeight(decimal? weight)
    {
        Weight = weight;
    }
}
