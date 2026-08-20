using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Reports;
using Application.Reports.Queries.GetAnalyticsByGroups;
using Application.Reports.Queries.GetAnalyticsByPeriod;
using Application.Reports.Queries.Shared;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Reports.Commands.ExportAnalyticsByPeriod;

internal sealed partial class ExportAnalyticsByPeriodCommandHandler(
    IApplicationDbContext dbContext,
    IReportGenerator reportGenerator,
    ILogger<ExportAnalyticsByPeriodCommandHandler> logger,
    IQueryHandler<GetAnalyticsByPeriodQuery, GetAnalyticsByPeriodQueryResult> analyticsQueryHandler,
    IQueryHandler<GetAnalyticsByGroupsQuery, List<GetAnalyticsByGroupsQueryResponse>> groupsQueryHandler)
    : ICommandHandler<ExportAnalyticsByPeriodCommand, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportAnalyticsByPeriodCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate form exists
            Form? form = await dbContext.Forms
                .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

            if (form is null)
            {
                return Result.Failure<byte[]>(FormErrors.NotFound(command.FormId));
            }

            // 2. Build one sheet per subject (discipline) present in the filtered submissions,
            // plus one sheet per teacher when a subject was taught by more than one teacher.
            Result<List<PeriodReportSheet>> sheetsResult = await BuildSheetsAsync(command, cancellationToken);

            if (sheetsResult.IsFailure)
            {
                return Result.Failure<byte[]>(sheetsResult.Error);
            }

            List<PeriodReportSheet> sheets = sheetsResult.Value;

            // 3. Handle empty data
            int totalQuestions = sheets.Sum(s => s.AnalyticsResult.Questions.Count);
            if (totalQuestions == 0)
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
                sheets,
                cancellationToken);

            LogGeneratedPeriodReportForFormForMidWithQuestionCountQuestions(logger, command.FormId, totalQuestions);

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

    private async Task<Result<List<PeriodReportSheet>>> BuildSheetsAsync(
        ExportAnalyticsByPeriodCommand command,
        CancellationToken cancellationToken)
    {
        GetAnalyticsByGroupsQuery disciplineQuery = new(
            command.FormId,
            command.FromDate,
            command.ToDate,
            GroupingType.Discipline,
            command.FilterSet);

        Result<List<GetAnalyticsByGroupsQueryResponse>> disciplineResult =
            await groupsQueryHandler.Handle(disciplineQuery, cancellationToken);

        if (disciplineResult.IsFailure)
        {
            return Result.Failure<List<PeriodReportSheet>>(disciplineResult.Error);
        }

        List<GetAnalyticsByGroupsQueryResponse> disciplineGroups = disciplineResult.Value;

        bool noDisciplineData = disciplineGroups.Count == 0 ||
            disciplineGroups.All(g => !Guid.TryParse(g.GroupKey, out Guid id) || id == Guid.Empty);

        if (noDisciplineData)
        {
            // No submission in range carries a discipline at all (grouping collapses everything
            // into a single "not specified" bucket rather than returning zero groups) or there's
            // no data whatsoever; fall back to a single aggregate sheet instead of a sheet titled
            // after the "not specified" placeholder.
            GetAnalyticsByPeriodQuery fallbackQuery = new(
                command.FormId,
                command.FromDate,
                command.ToDate,
                command.FilterSet);

            Result<GetAnalyticsByPeriodQueryResult> fallbackResult =
                await analyticsQueryHandler.Handle(fallbackQuery, cancellationToken);

            if (fallbackResult.IsFailure)
            {
                return Result.Failure<List<PeriodReportSheet>>(fallbackResult.Error);
            }

            // Empty name: the report generator falls back to its own default sheet title.
            return new List<PeriodReportSheet>
            {
                new(string.Empty, fallbackResult.Value),
            };
        }

        var sheets = new List<PeriodReportSheet>();

        foreach (GetAnalyticsByGroupsQueryResponse disciplineGroup in disciplineGroups)
        {
            sheets.Add(new PeriodReportSheet(disciplineGroup.GroupName, ToPeriodResult(disciplineGroup)));

            if (!Guid.TryParse(disciplineGroup.GroupKey, out Guid disciplineId) || disciplineId == Guid.Empty)
            {
                continue;
            }

            GetAnalyticsByGroupsQuery teacherQuery = new(
                command.FormId,
                command.FromDate,
                command.ToDate,
                GroupingType.Teacher,
                command.FilterSet with { DisciplineId = disciplineId });

            Result<List<GetAnalyticsByGroupsQueryResponse>> teacherResult =
                await groupsQueryHandler.Handle(teacherQuery, cancellationToken);

            if (teacherResult.IsFailure)
            {
                return Result.Failure<List<PeriodReportSheet>>(teacherResult.Error);
            }

            var namedTeacherGroups = teacherResult.Value
                .Where(g => Guid.TryParse(g.GroupKey, out Guid teacherId) && teacherId != Guid.Empty)
                .ToList();

            if (namedTeacherGroups.Count <= 1)
            {
                continue;
            }

            foreach (GetAnalyticsByGroupsQueryResponse teacherGroup in namedTeacherGroups)
            {
                string sheetName = $"{disciplineGroup.GroupName} ({ExtractSurname(teacherGroup.GroupName)})";
                sheets.Add(new PeriodReportSheet(sheetName, ToPeriodResult(teacherGroup)));
            }
        }

        return sheets;
    }

    private static string ExtractSurname(string fullName)
    {
        string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : fullName;
    }

    private static GetAnalyticsByPeriodQueryResult ToPeriodResult(GetAnalyticsByGroupsQueryResponse group)
    {
        var questions = group.QuestionStatistics
            .Select(stat => new GetAnalyticsByPeriodQueryResponse(
                stat.QuestionId,
                stat.QuestionText,
                stat.SatisfactionPercentage,
                stat.AverageScore,
                stat.StandardDeviation,
                stat.Rating,
                stat.ResponseCount))
            .ToList();

        return new GetAnalyticsByPeriodQueryResult(questions, group.Overall, group.SubmissionCount);
    }

    [LoggerMessage(LogLevel.Information, "No analytics data for form {formId}, generating empty report")]
    static partial void LogNoAnalyticsDataForFormForMidGeneratingEmptyReport(ILogger<ExportAnalyticsByPeriodCommandHandler> logger, Guid formId);

    [LoggerMessage(LogLevel.Information, "Generated period report for form {formId} with {questionCount} questions")]
    static partial void LogGeneratedPeriodReportForFormForMidWithQuestionCountQuestions(ILogger<ExportAnalyticsByPeriodCommandHandler> logger, Guid formId, int questionCount);

    [LoggerMessage(LogLevel.Error, "Error generating period report for form {formId}")]
    static partial void LogErrorGeneratingPeriodReportForFormForMid(ILogger<ExportAnalyticsByPeriodCommandHandler> logger, Guid formId);
}
