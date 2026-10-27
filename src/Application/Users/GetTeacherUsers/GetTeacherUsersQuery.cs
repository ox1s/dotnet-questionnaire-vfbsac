using Application.Abstractions.Messaging;

namespace Application.Users.GetTeacherUsers;

public sealed record GetTeacherUsersQuery() : IQuery<List<GetTeacherUsersQueryResponse>>;
