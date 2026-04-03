using SharedKernel;
namespace Domain.Questionnaires.Submissions;

public static class SubmissionErrors
{
    public static Error NotFound(Guid submissionId) => Error.NotFound(
        "Submissions.NotFound",
        $"{Resources.DomainErrors.Submissions_NotFound}, Id = '{submissionId}'");

    public static Error AnswerNotFound(Guid answerId) => Error.NotFound(
        "Submissions.AnswerNotFound",
        $"{Resources.DomainErrors.Submissions_AnswerNotFound}, Id = '{answerId}'");

    public static Error InvalidWeight(Guid questionId) => Error.Validation(
        "Submissions.InvalidWeight",
        $"{Resources.DomainErrors.Submissions_InvalidWeight}, QuestionId = '{questionId}'");

    public static Error AlreadySubmitted() => Error.Conflict(
        "Submissions.AlreadySubmitted",
        $"{Resources.DomainErrors.Submissions_AlreadySubmitted}");

    public static Error AnswerExists(Guid questionId) => Error.Failure(
        "Submissions.AnswerExists",
        $"{Resources.DomainErrors.Submissions_AnswerExists}. QuestionId = {questionId}");
}
