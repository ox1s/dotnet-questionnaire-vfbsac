using Application.Abstractions.Messaging;

namespace Application.Reports.Queries.GetAdvices;

public sealed record GetAdvicesQuery(Guid FormId, Guid? TeacherId) : IQuery<List<GetAdvicesQueryResponse>>;
