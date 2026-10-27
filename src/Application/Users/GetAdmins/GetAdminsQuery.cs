using Application.Abstractions.Messaging;

namespace Application.Users.GetAdmins;

public sealed record GetAdminsQuery() : IQuery<List<GetAdminsQueryResponse>>;
