using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Reports.Commands.ExportAnalyticsByPeriods;

internal sealed partial class ExportAnalyticsByPeriodsCommandHandler(
    IApplicationDbContext dbContext,
    IReportGenerator reportGenerator,
    ILogger<ExportAnalyticsByPeriodsCommandHandler> logger,
    IQueryHandler<GetAnalyticsByPeriodsQuery, List<GetAnalyticsByPeriodsQueryResponse>> analyticsQueryHandler)
    : ICommandHandler<ExportAnalyticsByPeriodsCommand, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportAnalyticsByPeriodsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Execute analytics query
            GetAnalyticsByPeriodsQuery analyticsQuery = new(
                command.FormId,
                command.Periods);

            Result<List<GetAnalyticsByPeriodsQueryResponse>> analyticsResult = await analyticsQueryHandler.Handle(analyticsQuery, cancellationToken);

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
            byte[] documentBytes = await reportGenerator.GeneratePeriodsComparisonReportAsync(
                form.Title,
                analyticsResult.Value,
                cancellationToken);

            LogGeneratedPeriodsComparisonReportForFormForMidWithPeriodCountPeriods(logger, command.FormId, analyticsResult.Value.Count);

            return documentBytes;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogErrorGeneratingPeriodsComparisonReportForFormForMid(logger, ex, command.FormId);

            return Result.Failure<byte[]>(
                Error.Failure("Report.GenerationFailed", "Failed to generate report"));
        }
    }

    [LoggerMessage(LogLevel.Information, "No analytics data for form {formId}, generating empty report")]
    static partial void LogNoAnalyticsDataForFormForMidGeneratingEmptyReport(
        ILogger<ExportAnalyticsByPeriodsCommandHandler> logger,
        Guid formId);

    [LoggerMessage(LogLevel.Information, "Generated periods comparison report for form {formId} with {periodCount} periods")]
    static partial void LogGeneratedPeriodsComparisonReportForFormForMidWithPeriodCountPeriods(
        ILogger<ExportAnalyticsByPeriodsCommandHandler> logger,
        Guid formId,
        int periodCount);

    [LoggerMessage(LogLevel.Error, "Error generating periods comparison report for form {formId}")]
    static partial void LogErrorGeneratingPeriodsComparisonReportForFormForMid(
        ILogger<ExportAnalyticsByPeriodsCommandHandler> logger,
        Exception exception,
        Guid formId);
}
