using Application.Abstractions.Messaging;

namespace Application.Users.CreateTeacher;

public sealed record CreateTeacherUserCommand(
    Guid TeacherId,
    string Login,
    string Password
) : ICommand<Guid>;
