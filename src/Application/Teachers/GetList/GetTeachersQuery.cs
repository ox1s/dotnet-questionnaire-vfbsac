using Application.Abstractions.Messaging;

namespace Application.Teachers.GetList;

public sealed record GetTeachersQuery() : IQuery<List<GetTeachersQueryResponse>>;
