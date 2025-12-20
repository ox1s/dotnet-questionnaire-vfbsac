using Application.Abstractions.Messaging;

namespace Application.Departments.GetList;

public sealed record GetDepartmentsQuery() : IQuery<List<DepartmentResponse>>;

public sealed record DepartmentResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
