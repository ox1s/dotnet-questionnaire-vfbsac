namespace Application.Users.GetGroups;

public sealed record GroupResponse(
    Guid Id,
    string Login,
    string DisplayName);
