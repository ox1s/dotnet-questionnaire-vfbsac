namespace Domain.Questionnaires.Submissions;

public record SubmissionContext(
    Guid? DisciplineId = null,
    Guid? TeacherId = null,
    Guid? DepartmentId = null,
    Guid? SpecialityId = null,
    Guid? SpecializationId = null,
    string? OrganizationName = null,
    string? EducationForm = null,
    string? EmployeeCategory = null,
    string? Position = null);
