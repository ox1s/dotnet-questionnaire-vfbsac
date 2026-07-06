using Application.Abstractions.Messaging;

namespace Application.Specializations.GetList;

public sealed record GetSpecializationsQuery() : IQuery<List<GetSpecializationsQueryResponse>>;
