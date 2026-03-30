using SharedKernel;

namespace Domain.Questionnaires.Forms;

public sealed class Question : Entity, ISoftDeletable
{
    public string Text { get; private set; }
    public QuestionType Type { get; private set; }
    public int Order { get; private set; }
    public bool IsDeleted { get; set; }

    public Guid FormId { get; private set; }
    
    private Question() { } // EF Core
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
            return Result.Failure<Question>(QuestionErrors.OrderInvalid);
        }

        return new Question(Guid.NewGuid(), formId, text.Trim(), type, order);
    }
}
