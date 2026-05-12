using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAnalyticsByGroups;

internal sealed class GetAnalyticsByGroupsQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetAnalyticsByGroupsQuery, List<GetAnalyticsByGroupsQueryResponse>>
{
    public async Task<Result<List<GetAnalyticsByGroupsQueryResponse>>> Handle(
        GetAnalyticsByGroupsQuery query,
        CancellationToken cancellationToken)
    {
        bool formExists = await context.Forms
            .AnyAsync(f => f.Id == query.FormId, cancellationToken);

        if (!formExists)
        {
            return Result.Failure<List<GetAnalyticsByGroupsQueryResponse>>(
                FormErrors.NotFound(query.FormId));
        }

        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s => s.FormId == query.FormId);

        IQueryable<Submission> filteredQuery = SubmissionFilterHelper.ApplyFilters(
            submissionsQuery,
            query.FilterSet);

        DateTime normalizedToDate = query.ToDate.AddDays(1);
        
        IQueryable<Submission> filteredByDate = filteredQuery
            .Where(s => s.SubmittedAt >= query.FromDate && s.SubmittedAt < normalizedToDate);

        // Get submission data with grouping fields only (no full entity load)
        List<SubmissionProjection> submissionsWithGrouping = await filteredByDate
            .Select(s => new SubmissionProjection(
                s.Id,
                s.Context.DepartmentId,
                s.Context.DisciplineId,
                s.Context.SpecialityId,
                s.Context.SpecializationId,
                s.Context.EducationForm,
                s.Context.EmployeeCategory,
                s.Context.TeacherId))
            .ToListAsync(cancellationToken);

        if (submissionsWithGrouping.Count == 0)
        {
            return new List<GetAnalyticsByGroupsQueryResponse>();
        }

        List<SubmissionGroup> groupedSubmissions = GroupSubmissions(submissionsWithGrouping, query.GroupBy);

        var submissionIds = submissionsWithGrouping.Select(s => s.Id).ToList();

        var answersGroupedByQuestion = await context.Answers
            .AsNoTracking()
            .Where(a => submissionIds.Contains(a.SubmissionId) &&
                       a.Value == null &&
                       a.NumericValue != null)
            .GroupBy(a => new { a.SubmissionId, a.QuestionId })
            .Select(g => new
            {
                g.Key.SubmissionId,
                g.Key.QuestionId,
                Value = g.Select(a => a.NumericValue!.Value).First()
            })
            .ToListAsync(cancellationToken);

        var questionIds = answersGroupedByQuestion.Select(a => a.QuestionId).Distinct().ToList();

        Dictionary<Guid, string> questions = await context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => new { q.Id, q.Text })
            .ToDictionaryAsync(q => q.Id, q => q.Text, cancellationToken);

        var responses = groupedSubmissions
            .Select(group =>
            {
                var groupSubmissionIds = group.SubmissionIds.ToHashSet();

                var questionStats = answersGroupedByQuestion
                    .Where(a => groupSubmissionIds.Contains(a.SubmissionId))
                    .GroupBy(a => a.QuestionId)
                    .Select(qGroup =>
                    {
                        var values = qGroup.Select(a => a.Value).ToList();

                        return new QuestionStatistics(
                            QuestionId: qGroup.Key,
                            QuestionText: questions.GetValueOrDefault(qGroup.Key) ?? string.Empty,
                            Median: StatisticsCalculator.CalculateMedian(values),
                            Mean: StatisticsCalculator.CalculateMean(values),
                            Mode: StatisticsCalculator.CalculateMode(values),
                            StandardDeviation: StatisticsCalculator.CalculateStandardDeviation(values),
                            ResponseCount: values.Count);
                    })
                    .ToList();

                return new GetAnalyticsByGroupsQueryResponse(
                    GroupKey: group.Key,
                    GroupName: group.Name,
                    QuestionStatistics: questionStats);
            })
            .ToList();

        return responses;
    }

    private static List<SubmissionGroup> GroupSubmissions(
        List<SubmissionProjection> submissions,
        GroupingType groupBy)
    {
        return groupBy switch
        {
            GroupingType.Department => submissions
                .GroupBy(s => s.DepartmentId ?? Guid.Empty)
                .Select(g => new SubmissionGroup(
                    g.Key.ToString(),
                    $"Department {g.Key}",
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            GroupingType.Discipline => submissions
                .GroupBy(s => s.DisciplineId ?? Guid.Empty)
                .Select(g => new SubmissionGroup(
                    g.Key.ToString(),
                    $"Discipline {g.Key}",
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            GroupingType.Speciality => submissions
                .GroupBy(s => s.SpecialityId ?? Guid.Empty)
                .Select(g => new SubmissionGroup(
                    g.Key.ToString(),
                    $"Speciality {g.Key}",
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            GroupingType.Specialization => submissions
                .GroupBy(s => s.SpecializationId ?? Guid.Empty)
                .Select(g => new SubmissionGroup(
                    g.Key.ToString(),
                    $"Specialization {g.Key}",
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            GroupingType.EducationForm => submissions
                .GroupBy(s => s.EducationForm ?? "Unknown")
                .Select(g => new SubmissionGroup(
                    g.Key,
                    g.Key,
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            GroupingType.EmployeeCategory => submissions
                .GroupBy(s => s.EmployeeCategory ?? "Unknown")
                .Select(g => new SubmissionGroup(
                    g.Key,
                    g.Key,
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            GroupingType.Teacher => submissions
                .GroupBy(s => s.TeacherId ?? Guid.Empty)
                .Select(g => new SubmissionGroup(
                    g.Key.ToString(),
                    $"Teacher {g.Key}",
                    g.Select(s => s.Id).ToList()))
                .ToList(),

            _ => throw new ArgumentOutOfRangeException(nameof(groupBy))
        };
    }

    private sealed record SubmissionProjection(
        Guid Id,
        Guid? DepartmentId,
        Guid? DisciplineId,
        Guid? SpecialityId,
        Guid? SpecializationId,
        string? EducationForm,
        string? EmployeeCategory,
        Guid? TeacherId);

    private sealed record SubmissionGroup(
        string Key,
        string Name,
        List<Guid> SubmissionIds);
}
