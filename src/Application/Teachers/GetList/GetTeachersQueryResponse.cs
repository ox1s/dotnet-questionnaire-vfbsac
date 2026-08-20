namespace Application.Teachers.GetList;

public sealed record GetTeachersQueryResponse(
    Guid Id,
    string FullName,
    IReadOnlyCollection<Guid> DepartmentIds,
    bool IsDeleted);
