namespace Application.Teachers.GetList;

public sealed record TeacherResponse(Guid Id, string FullName, bool IsDeleted);
