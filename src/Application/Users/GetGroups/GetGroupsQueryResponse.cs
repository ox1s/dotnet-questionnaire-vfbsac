namespace Application.Users.GetGroups;

public sealed record GetGroupsQueryResponse(
    Guid Id,
    string Login,
    string DisplayName);
