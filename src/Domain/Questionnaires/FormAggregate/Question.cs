using SharedKernel;

namespace Domain.Questionnaires.FormAggregate;

public sealed class Question : Entity
{
    public Guid FormId { get; private set; }
    public string Text { get; private set; }
    public QuestionType Type { get; private set; }
    public int Order { get; private set; }

    private Question() { }

    private Question(Guid id, Guid formId, string text, QuestionType type, int order) : base(id)
    {
        FormId = formId;
        Text = text;
        Type = type;
        Order = order;
    }

    public static Result<Question> Create(Guid formId, string text, QuestionType type, int order)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<Question>(Error.NullValue);
        }

        if (order < 0)
        {
            return Result.Failure<Question>(Error.Failure(
                "Questions.OrderInvalid",
                "Order must be non-negative"));
        }

        return new Question(Guid.NewGuid(), formId, text.Trim(), type, order);
    }

    public void UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Text = text.Trim();
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
        {
            return;
        }

        Order = order;
    }
}
