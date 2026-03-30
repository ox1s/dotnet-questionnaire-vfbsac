using Application.Abstractions.Messaging;

namespace Application.Specializations.GetList;

public sealed record GetSpecializationsQuery() : IQuery<List<SpecializationResponse>>;
public sealed record SpecializationResponse(Guid Id, string Name, Guid SpecialityId);
