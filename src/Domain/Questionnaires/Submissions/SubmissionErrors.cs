using SharedKernel;

namespace Domain.Questionnaires.Submissions;

public static class SubmissionErrors
{
    public static Error NotFound(Guid submissionId) => Error.NotFound(
        "Submissions.NotFound",
        $"The submission with the Id = '{submissionId}' was not found");

    public static Error AnswerNotFound(Guid answerId) => Error.NotFound(
        "Submissions.AnswerNotFound",
        $"The answer with the Id = '{answerId}' was not found");
    public static Error InvalidWeight(Guid questionId) => Error.Validation(
    "Submissions.InvalidWeight",
    $"The value cannot be greater than the weight for question '{questionId}'.");

    public static Error AlreadySubmitted() => Error.Conflict(
        "Submissions.AlreadySubmitted",
        "Вы уже отправляли ответ на эту анкету с выбранными параметрами.");
}
