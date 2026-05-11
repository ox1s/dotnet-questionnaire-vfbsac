namespace Application.Departments.GetList;

public sealed record GetDepartmentQueryResponse(Guid Id, string Name, bool IsDeleted);
