using Application.Abstractions.Messaging;

namespace Application.Users.GetGroups;

public sealed record GetGroupsQuery() : IQuery<List<GetGroupsQueryResponse>>;
