using Domain.Questionnaires.FormAggregate.Events;
using SharedKernel;

namespace Domain.Questionnaires.FormAggregate;

public sealed class Form : AggregateRoot
{
    public string Title { get; private set; }
    public bool IsActive { get; private set; }
    public List<FilterField>? RequiredFilters { get; private set; }

    private readonly List<Question> _questions = [];
    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

    private Form() { }

    private Form(Guid id, string title, bool isActive, List<FilterField>? requiredFilters) : base(id)
    {
        Title = title;
        IsActive = isActive;
        RequiredFilters = requiredFilters;
    }

    public static Result<Form> Create(string title, List<FilterField>? requiredFilters = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Form>(Error.NullValue);
        }

        var form = new Form(Guid.NewGuid(), title.Trim(), isActive: true, requiredFilters);
        form.RaiseDomainEvent(new FormCreatedDomainEvent(form.Id));

        return form;
    }

    public Result<Question> AddQuestion(string text, QuestionType type, int order)
    {
        if (_questions.Any(q => q.Order == order))
        {
            return Result.Failure<Question>(Error.Failure(
                "Forms.QuestionOrderExists",
                $"Question with order {order} already exists"));
        }

        Result<Question> questionResult = Question.Create(Id, text, type, order);
        if (questionResult.IsFailure)
        {
            return questionResult;
        }

        Question question = questionResult.Value;
        _questions.Add(question);

        return question;
    }

    public void RemoveQuestion(Guid questionId)
    {
        Question? question = _questions.FirstOrDefault(q => q.Id == questionId);
        if (question is not null)
        {
            _questions.Remove(question);
        }
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        Title = title.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            RaiseDomainEvent(new FormDeactivatedDomainEvent(Id));
        }
    }

    public void UpdateRequiredFilters(List<FilterField>? requiredFilters)
    {
        RequiredFilters = requiredFilters;
    }
}
