using Application.Abstractions.Messaging;
using Application.Reports.Queries.GetAnalyticsByPeriods;

namespace Application.Reports.Commands.ExportAnalyticsByPeriods;

public sealed record ExportAnalyticsByPeriodsCommand(
    Guid FormId,
    List<GetAnalyticsByPeriodsRequest> Periods)
    : ICommand<byte[]>;
