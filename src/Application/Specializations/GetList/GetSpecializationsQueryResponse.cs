namespace Application.Specializations.GetList;

public sealed record GetSpecializationsQueryResponse(Guid Id, string Name, Guid SpecialityId, bool IsDeleted);
