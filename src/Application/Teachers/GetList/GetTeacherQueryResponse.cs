namespace Application.Teachers.GetList;

public sealed record GetTeacherQueryResponse(Guid Id, string FullName, Guid? DepartmentId, bool IsDeleted);
