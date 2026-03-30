using Application.Abstractions.Messaging;

namespace Application.Disciplines.GetList;

public sealed record GetDisciplinesQuery : IQuery<List<DisciplineResponse>>;
public sealed record DisciplineResponse(Guid Id, string Name, Guid DepartmentId, bool IsDeleted);
