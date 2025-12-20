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

        return new Answer(Guid.NewGuid(), submissionId, questionId, value?.Trim(), numericValue, weight);
    }

    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
    }

    public void UpdateNumericValue(decimal? numericValue)
    {
        NumericValue = numericValue;
    }

    public void UpdateWeight(decimal? weight)
    {
        Weight = weight;
    }
}
