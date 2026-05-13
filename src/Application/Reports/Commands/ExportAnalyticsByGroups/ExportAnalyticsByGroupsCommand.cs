using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.Shared;

namespace Application.Reports.Commands.ExportAnalyticsByGroups;

public sealed record ExportAnalyticsByGroupsCommand(
    Guid FormId,
    DateTime FromDate,
    DateTime ToDate,
    GroupingType GroupBy,
    AnalyticsFilterSet FilterSet)
    : ICommand<byte[]>;
