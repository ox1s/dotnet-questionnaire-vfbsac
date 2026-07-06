using Application.Abstractions.Messaging;

namespace Application.Departments.GetList;

public sealed record GetDepartmentsQuery() : IQuery<List<GetDepartmentsQueryResponse>>;
