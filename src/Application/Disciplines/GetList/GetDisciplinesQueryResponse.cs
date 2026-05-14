namespace Application.Disciplines.GetList;

public sealed record GetDisciplinesQueryResponse(Guid Id, string Name, Guid DepartmentId, bool IsDeleted);
