using Application.Abstractions.Data;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Application.Reports.Queries.GetAnalytics;

internal sealed class SubmissionQueryBuilder(IApplicationDbContext context)
{
    public IQueryable<Submission> BuildQuery(
        Guid formId,
        DateTime dateFrom,
        DateTime dateTo,
        AnalyticsFilterSet filters)
    {
        DateTime normalizedFrom = dateFrom.Date;
        DateTime normalizedToExclusive = dateTo.Date.AddDays(1);

        IQueryable<Submission> query = context.Submissions
            .AsNoTracking()
            .Where(s =>
                s.FormId == formId &&
                s.SubmittedAt >= normalizedFrom &&
                s.SubmittedAt < normalizedToExclusive);

        return ApplyFilters(query, filters);
    }

    private static IQueryable<Submission> ApplyFilters(
        IQueryable<Submission> query,
        AnalyticsFilterSet filters)
    {
        if (filters.DisciplineId.HasValue)
        {
            query = query.Where(s => s.Context.DisciplineId == filters.DisciplineId);
        }

        if (filters.TeacherId.HasValue)
        {
            query = query.Where(s => s.Context.TeacherId == filters.TeacherId);
        }

        if (filters.DepartmentId.HasValue)
        {
            query = query.Where(s => s.Context.DepartmentId == filters.DepartmentId);
        }

        if (filters.SpecialityId.HasValue)
        {
            query = query.Where(s => s.Context.SpecialityId == filters.SpecialityId);
        }

        if (filters.SpecializationId.HasValue)
        {
            query = query.Where(s => s.Context.SpecializationId == filters.SpecializationId);
        }

        if (!string.IsNullOrWhiteSpace(filters.OrganizationName))
        {
            query = query.Where(s =>
                s.Context.OrganizationName != null &&
                EF.Functions.Like(s.Context.OrganizationName, $"%{filters.OrganizationName}%"));
        }

        return query;
    }
}
