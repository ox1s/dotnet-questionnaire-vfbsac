namespace Application.Reports.Queries.GetAdvices;

public sealed record GetAdvicesQueryResponse(
    string Text,
    Guid? TeacherId,
    Guid? DepartmentId);
