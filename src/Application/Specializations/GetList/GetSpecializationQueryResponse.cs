namespace Application.Specializations.GetList;

public sealed record GetSpecializationQueryResponse(Guid Id, string Name, Guid SpecialityId, bool IsDeleted);
