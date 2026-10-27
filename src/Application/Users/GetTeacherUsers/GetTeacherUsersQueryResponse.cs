namespace Application.Users.GetTeacherUsers;

public sealed record GetTeacherUsersQueryResponse(
    Guid Id,
    string Login,
    string DisplayName,
    Guid TeacherId);
