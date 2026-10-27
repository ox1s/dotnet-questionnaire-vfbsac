namespace Application.Users.GetStaff;

public sealed record GetStaffQueryResponse(
    Guid Id,
    string Login,
    string DisplayName);
