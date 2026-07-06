using Domain.Questionnaires.Submissions;

namespace Application.Reports.Queries.Shared;

internal static class SubmissionFilterHelper
{
    public static IQueryable<Submission> ApplyFilters(
        IQueryable<Submission> query,
        AnalyticsFilterSet? filterSet)
    {
        if (filterSet is null)
        {
            return query;
        }

        if (filterSet.DepartmentId.HasValue)
        {
            query = query.Where(s => s.Context.DepartmentId == filterSet.DepartmentId);
        }

        if (filterSet.DisciplineId.HasValue)
        {
            query = query.Where(s => s.Context.DisciplineId == filterSet.DisciplineId);
        }

        if (filterSet.EducationForm != null)
        {
            query = query.Where(s =>
                s.Context.EducationForm != null &&
                s.Context.EducationForm.Equals(filterSet.EducationForm, StringComparison.Ordinal));
        }

        if (filterSet.SpecializationId.HasValue)
        {
            query = query.Where(s => s.Context.SpecializationId == filterSet.SpecializationId);
        }

        if (filterSet.SpecialityId.HasValue)
        {
            query = query.Where(s => s.Context.SpecialityId == filterSet.SpecialityId);
        }

        if (filterSet.EmployeeCategory != null)
        {
            query = query.Where(s =>
                s.Context.EmployeeCategory != null &&
                s.Context.EmployeeCategory.Equals(filterSet.EmployeeCategory, StringComparison.Ordinal));
        }

        if (filterSet.TeacherId.HasValue)
        {
            query = query.Where(s => s.Context.TeacherId == filterSet.TeacherId);
        }

        if (filterSet.OrganizationName != null)
        {
            query = query.Where(s =>
                s.Context.OrganizationName != null &&
                s.Context.OrganizationName.Equals(filterSet.OrganizationName, StringComparison.Ordinal));
        }

        if (filterSet.Position != null)
        {
            query = query.Where(s =>
                s.Context.Position != null &&
                s.Context.Position.Equals(filterSet.Position, StringComparison.Ordinal));
        }

        return query;
    }
}
