using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Reports.Commands.ExportAnalyticsByGroups;

internal sealed partial class ExportAnalyticsByGroupsCommandHandler(
    IApplicationDbContext dbContext,
    IWordReportGenerator reportGenerator,
    ILogger<ExportAnalyticsByGroupsCommandHandler> logger,
    IQueryHandler<GetAnalyticsByGroupsQuery, List<GetAnalyticsByGroupsQueryResponse>> analyticsQueryHandler)
    : ICommandHandler<ExportAnalyticsByGroupsCommand, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportAnalyticsByGroupsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Execute analytics query
            GetAnalyticsByGroupsQuery analyticsQuery = new(
                command.FormId,
                command.FromDate,
                command.ToDate,
                command.GroupBy,
                command.FilterSet);

            Result<List<GetAnalyticsByGroupsQueryResponse>> analyticsResult = await analyticsQueryHandler.Handle(analyticsQuery, cancellationToken);

            if (analyticsResult.IsFailure)
            {
                return Result.Failure<byte[]>(analyticsResult.Error);
            }

            // 2. Validate form exists
            Form? form = await dbContext.Forms
                .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

            if (form is null)
            {
                return Result.Failure<byte[]>(FormErrors.NotFound(command.FormId));
            }

            // 3. Handle empty data
            if (analyticsResult.Value.Count == 0)
            {
                LogNoAnalyticsDataForFormForMidGeneratingEmptyReport(logger, command.FormId);
            }

            // 4. Generate document
            byte[] documentBytes = await reportGenerator.GenerateGroupsComparisonReportAsync(
                form.Title,
                analyticsResult.Value,
                cancellationToken);

            LogGeneratedGroupsComparisonReportForFormForMidWithGroupCountGroups(logger, command.FormId, analyticsResult.Value.Count);

            return documentBytes;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error generating groups comparison report for form {FormId}",
                command.FormId);

            return Result.Failure<byte[]>(
                Error.Failure("Report.GenerationFailed", "Failed to generate report"));
        }
    }

    [LoggerMessage(LogLevel.Information, "No analytics data for form {formId}, generating empty report")]
    static partial void LogNoAnalyticsDataForFormForMidGeneratingEmptyReport(ILogger<ExportAnalyticsByGroupsCommandHandler> logger, Guid formId);

    [LoggerMessage(LogLevel.Information, "Generated groups comparison report for form {formId} with {groupCount} groups")]
    static partial void LogGeneratedGroupsComparisonReportForFormForMidWithGroupCountGroups(ILogger<ExportAnalyticsByGroupsCommandHandler> logger, Guid formId, int groupCount);
}
