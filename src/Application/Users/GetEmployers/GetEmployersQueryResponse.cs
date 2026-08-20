namespace Application.Users.GetEmployers;

public sealed record GetEmployersQueryResponse(
    Guid Id,
    string Login,
    string DisplayName,
    string? OrganizationName);
