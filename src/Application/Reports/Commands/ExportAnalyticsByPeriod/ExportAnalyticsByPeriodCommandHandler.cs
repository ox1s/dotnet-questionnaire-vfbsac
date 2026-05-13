using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Reports.Commands.ExportAnalyticsByPeriod;

internal sealed class ExportAnalyticsByPeriodCommandHandler(
    IApplicationDbContext dbContext,
    IWordReportGenerator reportGenerator,
    ILogger<ExportAnalyticsByPeriodCommandHandler> logger,
    IQueryHandler<GetAnalyticsByPeriodQuery, List<GetAnalyticsByPeriodQueryResponse>> analyticsQueryHandler)
    : ICommandHandler<ExportAnalyticsByPeriodCommand, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportAnalyticsByPeriodCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Execute analytics query
            GetAnalyticsByPeriodQuery analyticsQuery = new(
                command.FormId,
                command.FromDate,
                command.ToDate,
                command.FilterSet);

            Result<List<GetAnalyticsByPeriodQueryResponse>> analyticsResult = await analyticsQueryHandler.Handle(analyticsQuery, cancellationToken);

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

            // 4. Resolve filter names
            Dictionary<string, string> resolvedFilters = await Queries.Shared.EntityNameResolver.ResolveFilterNamesAsync(
                command.FilterSet,
                dbContext,
                logger,
                cancellationToken);

            // 5. Generate document
            byte[] documentBytes = await reportGenerator.GeneratePeriodReportAsync(
                form.Title,
                command.FromDate,
                command.ToDate,
                resolvedFilters,
                analyticsResult.Value,
                cancellationToken);

            logger.LogInformation(
                "Generated period report for form {FormId} with {QuestionCount} questions",
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
                "Error generating period report for form {FormId}",
                command.FormId);

            return Result.Failure<byte[]>(
                Error.Failure("Report.GenerationFailed", "Failed to generate report"));
        }
    }
}
