using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;

namespace Application.Reports.Commands.ExportAnalyticsByPeriod;

public sealed record ExportAnalyticsByPeriodCommand(
    Guid FormId,
    DateTime FromDate,
    DateTime ToDate,
    AnalyticsFilterSet FilterSet)
    : ICommand<byte[]>;
