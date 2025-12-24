using Application.Abstractions.Messaging;

namespace Application.Submissions.Create;

public sealed record CreateSubmissionCommand(
    Guid FormId,
    Guid UserId,
    List<AnswerRequest> Answers,
    Guid? DisciplineId = null,
    Guid? TeacherId = null,
    Guid? DepartmentId = null,
    Guid? SpecialityId = null,
    Guid? SpecializationId = null,
    string? OrganizationName = null,
    string? EducationForm = null,
    string? EmployeeCategory = null,
    string? Position = null)
    : ICommand<Guid>;
