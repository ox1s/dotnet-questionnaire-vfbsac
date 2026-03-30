using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetComparative;
using Domain.Questionnaires.Forms;

namespace Application.Reports.Queries.GetUnitComparative;

public sealed record GetUnitComparativeReportQuery(
    Guid FormId,
    FilterField UnitType,
    Guid UnitAId,
    Guid UnitBId,
    DateTime? From = null,
    DateTime? To = null
) : IQuery<List<ComparativeReportResponse>>;

