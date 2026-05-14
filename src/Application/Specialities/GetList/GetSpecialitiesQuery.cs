using Application.Abstractions.Messaging;

namespace Application.Specialities.GetList;

public sealed record GetSpecialitiesQuery() : IQuery<List<GetSpecialitiesQueryResponse>>;
