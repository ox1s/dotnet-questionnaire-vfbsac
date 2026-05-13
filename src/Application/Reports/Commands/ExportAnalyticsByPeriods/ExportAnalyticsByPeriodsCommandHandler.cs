using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByPeriods;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Reports.Commands.ExportAnalyticsByPeriods;

internal sealed class ExportAnalyticsByPeriodsCommandHandler(
    IApplicationDbContext dbContext,
    IWordReportGenerator reportGenerator,
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
                logger.LogInformation(
                    "No analytics data for form {FormId}, generating empty report",
                    command.FormId);
            }

            // 4. Generate document
            byte[] documentBytes = await reportGenerator.GeneratePeriodsComparisonReportAsync(
                form.Title,
                analyticsResult.Value,
                cancellationToken);

            logger.LogInformation(
                "Generated periods comparison report for form {FormId} with {PeriodCount} periods",
                command.FormId,
                analyticsResult.Value.Count);

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
                "Error generating periods comparison report for form {FormId}",
                command.FormId);

            return Result.Failure<byte[]>(
                Error.Failure("Report.GenerationFailed", "Failed to generate report"));
        }
    }
}
