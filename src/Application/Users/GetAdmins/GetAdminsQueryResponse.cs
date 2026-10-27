namespace Application.Users.GetAdmins;

public sealed record GetAdminsQueryResponse(
    Guid Id,
    string Login,
    string DisplayName);
