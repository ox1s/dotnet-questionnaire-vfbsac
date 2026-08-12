using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Reports.Commands.ExportAnalyticsByPeriod;

internal sealed partial class ExportAnalyticsByPeriodCommandHandler(
    IApplicationDbContext dbContext,
    IReportGenerator reportGenerator,
    ILogger<ExportAnalyticsByPeriodCommandHandler> logger,
    IQueryHandler<GetAnalyticsByPeriodQuery, GetAnalyticsByPeriodQueryResult> analyticsQueryHandler)
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

            Result<GetAnalyticsByPeriodQueryResult> analyticsResult = await analyticsQueryHandler.Handle(analyticsQuery, cancellationToken);

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
            if (analyticsResult.Value.Questions.Count == 0)
            {
                LogNoAnalyticsDataForFormForMidGeneratingEmptyReport(logger, command.FormId);
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

            LogGeneratedPeriodReportForFormForMidWithQuestionCountQuestions(logger, command.FormId, analyticsResult.Value.Questions.Count);

            return documentBytes;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            LogErrorGeneratingPeriodReportForFormForMid(logger, command.FormId);

            return Result.Failure<byte[]>(
                Error.Failure("Report.GenerationFailed", "Failed to generate report"));
        }
    }

    [LoggerMessage(LogLevel.Information, "No analytics data for form {formId}, generating empty report")]
    static partial void LogNoAnalyticsDataForFormForMidGeneratingEmptyReport(ILogger<ExportAnalyticsByPeriodCommandHandler> logger, Guid formId);

    [LoggerMessage(LogLevel.Information, "Generated period report for form {formId} with {questionCount} questions")]
    static partial void LogGeneratedPeriodReportForFormForMidWithQuestionCountQuestions(ILogger<ExportAnalyticsByPeriodCommandHandler> logger, Guid formId, int questionCount);

    [LoggerMessage(LogLevel.Error, "Error generating period report for form {formId}")]
    static partial void LogErrorGeneratingPeriodReportForFormForMid(ILogger<ExportAnalyticsByPeriodCommandHandler> logger, Guid formId);
}
