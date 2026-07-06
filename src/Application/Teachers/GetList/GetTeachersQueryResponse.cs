namespace Application.Teachers.GetList;

public sealed record GetTeachersQueryResponse(Guid Id, string FullName, Guid? DepartmentId, bool IsDeleted);
