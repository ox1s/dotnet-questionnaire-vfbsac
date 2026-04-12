namespace Application.Reports.Queries.GetAdvices;

public record AdvicesQueryResponse(
    string Text,
    Guid? TeacherId,
    Guid? DepartmentId);
