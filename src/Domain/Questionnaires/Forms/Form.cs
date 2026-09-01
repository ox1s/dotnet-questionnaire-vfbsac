using Domain.User;
using SharedKernel;

namespace Domain.Questionnaires.Forms;

public sealed class Form : Entity, ISoftDeletable
{
    public string Title { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; set; }

    public List<FilterField>? RequiredFilters { get; private set; }

    /// <summary>
    /// The single role this form is intended for (e.g. Employer). Null means the
    /// form is general-purpose and visible to every role, which is the default
    /// and preserves prior behavior for forms created before this field existed.
    /// </summary>
    public UserRole? TargetRole { get; private set; }
    private readonly List<Question> _questions = [];
    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

    private Form() { } // EF Core
    private Form(Guid id, string title, bool isActive, List<FilterField>? requiredFilters, UserRole? targetRole) : base(id)
    {
        Title = title;
        IsActive = isActive;
        RequiredFilters = requiredFilters;
        TargetRole = targetRole;
    }

    public static Result<Form> Create(string title, List<FilterField>? requiredFilters = null, UserRole? targetRole = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Form>(Error.NullValue);
        }

        var form = new Form(Guid.NewGuid(), title.Trim(), isActive: true, requiredFilters, targetRole);

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

        return Result.Success();
    }
}
