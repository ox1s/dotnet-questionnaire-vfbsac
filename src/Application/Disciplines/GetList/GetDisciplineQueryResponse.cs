namespace Application.Disciplines.GetList;

public sealed record GetDisciplineQueryResponse(Guid Id, string Name, Guid DepartmentId, bool IsDeleted);
