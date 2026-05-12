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

        List<SubmissionGroup> groupedSubmissions = await GroupSubmissionsAsync(
            submissionsWithGrouping, 
            query.GroupBy, 
            context, 
            cancellationToken);

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

    private static async Task<List<SubmissionGroup>> GroupSubmissionsAsync(
        List<SubmissionProjection> submissions,
        GroupingType groupBy,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        return groupBy switch
        {
            GroupingType.Department => await GroupByDepartmentAsync(submissions, context, cancellationToken),
            GroupingType.Discipline => await GroupByDisciplineAsync(submissions, context, cancellationToken),
            GroupingType.Speciality => await GroupBySpecialityAsync(submissions, context, cancellationToken),
            GroupingType.Specialization => await GroupBySpecializationAsync(submissions, context, cancellationToken),
            GroupingType.Teacher => await GroupByTeacherAsync(submissions, context, cancellationToken),
            
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

            _ => throw new ArgumentOutOfRangeException(nameof(groupBy))
        };
    }

    private static async Task<List<SubmissionGroup>> GroupByDepartmentAsync(
        List<SubmissionProjection> submissions,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var departmentIds = submissions
            .Select(s => s.DepartmentId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> departments = await context.Departments
            .AsNoTracking()
            .Where(d => departmentIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Name })
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

        return submissions
            .GroupBy(s => s.DepartmentId ?? Guid.Empty)
            .Select(g => new SubmissionGroup(
                g.Key.ToString(),
                g.Key == Guid.Empty ? "Не указано" : departments.GetValueOrDefault(g.Key) ?? g.Key.ToString(),
                g.Select(s => s.Id).ToList()))
            .ToList();
    }

    private static async Task<List<SubmissionGroup>> GroupByDisciplineAsync(
        List<SubmissionProjection> submissions,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var disciplineIds = submissions
            .Select(s => s.DisciplineId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> disciplines = await context.Disciplines
            .AsNoTracking()
            .Where(d => disciplineIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Name })
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

        return submissions
            .GroupBy(s => s.DisciplineId ?? Guid.Empty)
            .Select(g => new SubmissionGroup(
                g.Key.ToString(),
                g.Key == Guid.Empty ? "Не указано" : disciplines.GetValueOrDefault(g.Key) ?? g.Key.ToString(),
                g.Select(s => s.Id).ToList()))
            .ToList();
    }

    private static async Task<List<SubmissionGroup>> GroupBySpecialityAsync(
        List<SubmissionProjection> submissions,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var specialityIds = submissions
            .Select(s => s.SpecialityId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> specialities = await context.Specialities
            .AsNoTracking()
            .Where(s => specialityIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        return submissions
            .GroupBy(s => s.SpecialityId ?? Guid.Empty)
            .Select(g => new SubmissionGroup(
                g.Key.ToString(),
                g.Key == Guid.Empty ? "Не указано" : specialities.GetValueOrDefault(g.Key) ?? g.Key.ToString(),
                g.Select(s => s.Id).ToList()))
            .ToList();
    }

    private static async Task<List<SubmissionGroup>> GroupBySpecializationAsync(
        List<SubmissionProjection> submissions,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var specializationIds = submissions
            .Select(s => s.SpecializationId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> specializations = await context.Specializations
            .AsNoTracking()
            .Where(s => specializationIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        return submissions
            .GroupBy(s => s.SpecializationId ?? Guid.Empty)
            .Select(g => new SubmissionGroup(
                g.Key.ToString(),
                g.Key == Guid.Empty ? "Не указано" : specializations.GetValueOrDefault(g.Key) ?? g.Key.ToString(),
                g.Select(s => s.Id).ToList()))
            .ToList();
    }

    private static async Task<List<SubmissionGroup>> GroupByTeacherAsync(
        List<SubmissionProjection> submissions,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var teacherIds = submissions
            .Select(s => s.TeacherId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> teachers = await context.Teachers
            .AsNoTracking()
            .Where(t => teacherIds.Contains(t.Id))
            .Select(t => new { t.Id, t.FullName })
            .ToDictionaryAsync(t => t.Id, t => t.FullName, cancellationToken);

        return submissions
            .GroupBy(s => s.TeacherId ?? Guid.Empty)
            .Select(g => new SubmissionGroup(
                g.Key.ToString(),
                g.Key == Guid.Empty ? "Не указано" : teachers.GetValueOrDefault(g.Key) ?? g.Key.ToString(),
                g.Select(s => s.Id).ToList()))
            .ToList();
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
