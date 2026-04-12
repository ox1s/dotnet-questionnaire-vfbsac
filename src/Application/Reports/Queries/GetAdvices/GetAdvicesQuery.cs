using Application.Abstractions.Messaging;

namespace Application.Reports.Queries.GetAdvices;

public record GetAdvicesQuery(Guid FormId, Guid? TeacherId) : IQuery<List<AdvicesQueryResponse>>;
