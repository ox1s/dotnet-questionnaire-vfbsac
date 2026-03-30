using Application.Abstractions.Messaging;

namespace Application.Specialities.GetList;

public sealed record GetSpecialitiesQuery() : IQuery<List<SpecialityResponse>>;
public sealed record SpecialityResponse(Guid Id, string Name, bool IsDeleted);
