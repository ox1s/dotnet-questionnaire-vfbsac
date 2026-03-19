using SharedKernel;

namespace Domain.Questionnaires.SubmissionAggregate;

public sealed class Submission : AggregateRoot
{
    public Guid FormId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public SubmissionContext Context { get; private set; }
    public string DeviceId { get; set; }
    public bool IsDeleted { get; set; }

    private readonly List<Answer> _answers = [];
    public IReadOnlyList<Answer> Answers => _answers.AsReadOnly();

    private Submission() { } // EF Core
    private Submission(Guid id, string deviceId, Guid formId, Guid userId, DateTime submittedAt, SubmissionContext context) : base(id)
    {
        FormId = formId;
        DeviceId = deviceId;
        UserId = userId;
        Context = context;
        SubmittedAt = submittedAt;
    }

    public static Result<Submission> Create(
        Guid formId,
        string deviceId,
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
            deviceId,
            formId,
            userId,
            DateTime.UtcNow,
            context);

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
    // TODO: Result
    public void UpdateContext(SubmissionContext context)
    {
        Context = context;
    }
}
