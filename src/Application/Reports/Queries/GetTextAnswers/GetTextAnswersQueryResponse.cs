namespace Application.Reports.Queries.GetTextAnswers;

public sealed record GetTextAnswersQueryResponse(
    Guid QuestionId,
    string QuestionText,
    string Value,
    DateTime SubmittedAt,
    Guid? TeacherId,
    string? TeacherName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? DisciplineId,
    string? DisciplineName);
