using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;

namespace Application.Reports.Queries.GetTextAnswers;

public sealed record GetTextAnswersQuery(
    Guid FormId,
    AnalyticsFilterSet FilterSet,
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null)
    : IQuery<List<GetTextAnswersQueryResponse>>;
