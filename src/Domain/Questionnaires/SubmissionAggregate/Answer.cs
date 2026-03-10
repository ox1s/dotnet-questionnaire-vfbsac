using SharedKernel;

namespace Domain.Questionnaires.SubmissionAggregate;

public sealed class Answer : Entity, ISoftDeletable
{
    public Guid SubmissionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string? Value { get; private set; }
    public decimal? NumericValue { get; private set; }
    public decimal? Weight { get; private set; }
    public bool IsDeleted { get; set; }

    private Answer() { } // EF

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
            return Result.Failure<Answer>(AnswerErrors.ValueRequired);
        }

        const int MinValue = 1;
        const int MaxValue = 10;

        if (numericValue.HasValue && (numericValue.Value < MinValue || numericValue.Value > MaxValue))
        {
            return Result.Failure<Answer>(AnswerErrors.InvalidScore(MinValue, MaxValue));
        }

        if (weight.HasValue && (weight.Value < MinValue || weight.Value > MaxValue))
        {
            return Result.Failure<Answer>(AnswerErrors.InvalidWeight(MinValue, MaxValue));
        }

        if (weight.HasValue && numericValue.HasValue && numericValue.Value > weight.Value)
        {
            return Result.Failure<Answer>(SubmissionErrors.InvalidWeight(questionId));
        }

        return new Answer(Guid.NewGuid(), submissionId, questionId, value?.Trim(), numericValue, weight);
    }
}
