using Application.Abstractions.Messaging;

namespace Application.Users.GetEmployers;

public sealed record GetEmployersQuery() : IQuery<List<GetEmployersQueryResponse>>;
