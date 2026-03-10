using Domain.Questionnaires.FormAggregate.Events;
using SharedKernel;

namespace Domain.Questionnaires.FormAggregate;

public sealed class Form : AggregateRoot, ISoftDeletable
{
    public string Title { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; set; }
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
            return Result.Failure<Question>(FormErrors.QuestionOrderExists(order));
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

    public Result RemoveQuestion(Guid questionId)
    {
        Question? question = _questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
        {
            return Result.Failure(FormErrors.QuestionNotFound(questionId));
        }

        _questions.Remove(question);
        return Result.Success();
    }

    public Result UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(Error.NullValue);
        }

        Title = title.Trim();
        return Result.Success();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(FormErrors.AlreadyDeactivated(Id));
        }
        IsActive = false;

        RaiseDomainEvent(new FormDeactivatedDomainEvent(Id));
        return Result.Success();
    }

    public void UpdateRequiredFilters(List<FilterField>? requiredFilters)
    {
        RequiredFilters = requiredFilters;
    }
}
