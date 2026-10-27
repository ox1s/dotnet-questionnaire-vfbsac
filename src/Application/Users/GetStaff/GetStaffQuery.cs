using Application.Abstractions.Messaging;

namespace Application.Users.GetStaff;

public sealed record GetStaffQuery() : IQuery<List<GetStaffQueryResponse>>;
