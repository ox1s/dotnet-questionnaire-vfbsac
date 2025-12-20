using Domain.Questionnaires.SubmissionAggregate.Events;
using SharedKernel;

namespace Domain.Questionnaires.SubmissionAggregate;

public sealed class Submission : AggregateRoot
{
    public Guid FormId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public SubmissionContext Context { get; private set; }

    private readonly List<Answer> _answers = [];
    public IReadOnlyList<Answer> Answers => _answers.AsReadOnly();

    private Submission() { }

    private Submission(Guid id, Guid formId, Guid userId, DateTime submittedAt, SubmissionContext context) : base(id)
    {
        FormId = formId;
        UserId = userId;
        Context = context;
        SubmittedAt = submittedAt;
    }

    public static Result<Submission> Create(
        Guid formId,
        Guid userId,
        Guid? disciplineId = null,
        Guid? teacherId = null,
        Guid? departmentId = null,
        Guid? specialityId = null,
        Guid? specializationId = null,
        string? organizationName = null)
    {
        var context = new SubmissionContext(
            disciplineId,
            teacherId,
            departmentId,
            specialityId,
            specializationId,
            organizationName);
        
        var submission = new Submission(
            Guid.NewGuid(),
            formId,
            userId,
            DateTime.UtcNow,
            context);
        
        submission.RaiseDomainEvent(new SubmissionCreatedDomainEvent(submission.Id, formId, userId));
        
        return submission;
    }

    public Result<Answer> AddAnswer(Guid questionId, string? value = null, decimal? numericValue = null, decimal? weight = null)
    {
        if (_answers.Any(a => a.QuestionId == questionId))
        {
            return Result.Failure<Answer>(Error.Failure(
                "Submissions.AnswerExists",
                $"Answer for question {questionId} already exists"));
        }

        Result<Answer> answerResult = Answer.Create(Id, questionId, value, numericValue, weight);
        if (answerResult.IsFailure)
        {
            return answerResult;
        }

        Answer answer = answerResult.Value;
        _answers.Add(answer);

        return answer;
    }

    public void RemoveAnswer(Guid answerId)
    {
        Answer? answer = _answers.FirstOrDefault(a => a.Id == answerId);
        if (answer is not null)
        {
            _answers.Remove(answer);
        }
    }

    public void UpdateContext(SubmissionContext context)
    {
        Context = context;
    }
}
