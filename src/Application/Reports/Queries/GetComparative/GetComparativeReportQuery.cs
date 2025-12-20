using Application.Abstractions.Messaging;

namespace Application.Reports.Queries.GetComparative;

public sealed record GetComparativeReportQuery(
    Guid FormId,
    DateTime PeriodA_Start,
    DateTime PeriodA_End,
    DateTime PeriodB_Start,
    DateTime PeriodB_End
) : IQuery<List<ComparativeReportResponse>>;
