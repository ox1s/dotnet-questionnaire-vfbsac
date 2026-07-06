using Application.Abstractions.Messaging;

namespace Application.Disciplines.GetList;

public sealed record GetDisciplinesQuery : IQuery<List<GetDisciplinesQueryResponse>>;
