using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Reports.Queries.Shared;

public static class EntityNameResolver
{
    public static async Task<Dictionary<string, string>> ResolveFilterNamesAsync(
        AnalyticsFilterSet filterSet,
        IApplicationDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> resolvedFilters = new();

        try
        {
            // Batch load all entities in parallel
            Task<Domain.College.Teachers.Teacher?> teacherTask = filterSet.TeacherId.HasValue
                ? dbContext.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == filterSet.TeacherId.Value, cancellationToken)
                : Task.FromResult<Domain.College.Teachers.Teacher?>(null);

            Task<Domain.College.Disciplines.Discipline?> disciplineTask = filterSet.DisciplineId.HasValue
                ? dbContext.Disciplines
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == filterSet.DisciplineId.Value, cancellationToken)
                : Task.FromResult<Domain.College.Disciplines.Discipline?>(null);

            Task<Domain.College.Departments.Department?> departmentTask = filterSet.DepartmentId.HasValue
                ? dbContext.Departments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == filterSet.DepartmentId.Value, cancellationToken)
                : Task.FromResult<Domain.College.Departments.Department?>(null);

            Task<Domain.College.Specialities.Speciality?> specialityTask = filterSet.SpecialityId.HasValue
                ? dbContext.Specialities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == filterSet.SpecialityId.Value, cancellationToken)
                : Task.FromResult<Domain.College.Specialities.Speciality?>(null);

            Task<Domain.College.Specializations.Specialization?> specializationTask = filterSet.SpecializationId.HasValue
                ? dbContext.Specializations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == filterSet.SpecializationId.Value, cancellationToken)
                : Task.FromResult<Domain.College.Specializations.Specialization?>(null);

            await Task.WhenAll(teacherTask, disciplineTask, departmentTask, specialityTask, specializationTask);

            // Add resolved names with GUID fallback
            if (filterSet.TeacherId.HasValue)
            {
                Domain.College.Teachers.Teacher? teacher = await teacherTask;
                string teacherName = teacher?.FullName ?? filterSet.TeacherId.Value.ToString();

                if (teacher is null)
                {
                    logger.LogWarning(
                        "Teacher with ID {TeacherId} not found, using GUID",
                        filterSet.TeacherId.Value);
                }

                resolvedFilters["Преподаватель"] = teacherName;
            }

            if (filterSet.DisciplineId.HasValue)
            {
                Domain.College.Disciplines.Discipline? discipline = await disciplineTask;
                string disciplineName = discipline?.Name ?? filterSet.DisciplineId.Value.ToString();

                if (discipline is null)
                {
                    logger.LogWarning(
                        "Discipline with ID {DisciplineId} not found, using GUID",
                        filterSet.DisciplineId.Value);
                }

                resolvedFilters["Дисциплина"] = disciplineName;
            }

            if (filterSet.DepartmentId.HasValue)
            {
                Domain.College.Departments.Department? department = await departmentTask;
                string departmentName = department?.Name ?? filterSet.DepartmentId.Value.ToString();

                if (department is null)
                {
                    logger.LogWarning(
                        "Department with ID {DepartmentId} not found, using GUID",
                        filterSet.DepartmentId.Value);
                }

                resolvedFilters["Кафедра"] = departmentName;
            }

            if (filterSet.SpecialityId.HasValue)
            {
                Domain.College.Specialities.Speciality? speciality = await specialityTask;
                string specialityName = speciality?.Name ?? filterSet.SpecialityId.Value.ToString();

                if (speciality is null)
                {
                    logger.LogWarning(
                        "Speciality with ID {SpecialityId} not found, using GUID",
                        filterSet.SpecialityId.Value);
                }

                resolvedFilters["Специальность"] = specialityName;
            }

            if (filterSet.SpecializationId.HasValue)
            {
                Domain.College.Specializations.Specialization? specialization = await specializationTask;
                string specializationName = specialization?.Name ?? filterSet.SpecializationId.Value.ToString();

                if (specialization is null)
                {
                    logger.LogWarning(
                        "Specialization with ID {SpecializationId} not found, using GUID",
                        filterSet.SpecializationId.Value);
                }

                resolvedFilters["Специализация"] = specializationName;
            }

            // Add text filters as-is
            if (!string.IsNullOrWhiteSpace(filterSet.OrganizationName))
            {
                resolvedFilters["Организация"] = filterSet.OrganizationName;
            }

            if (!string.IsNullOrWhiteSpace(filterSet.EducationForm))
            {
                resolvedFilters["Форма обучения"] = filterSet.EducationForm;
            }

            if (!string.IsNullOrWhiteSpace(filterSet.EmployeeCategory))
            {
                resolvedFilters["Категория сотрудника"] = filterSet.EmployeeCategory;
            }

            if (!string.IsNullOrWhiteSpace(filterSet.Position))
            {
                resolvedFilters["Должность"] = filterSet.Position;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error resolving filter names, using GUIDs as fallback");

            // Return GUIDs for all filters on error
            if (filterSet.TeacherId.HasValue)
            {
                resolvedFilters["Преподаватель"] = filterSet.TeacherId.Value.ToString();
            }

            if (filterSet.DisciplineId.HasValue)
            {
                resolvedFilters["Дисциплина"] = filterSet.DisciplineId.Value.ToString();
            }

            if (filterSet.DepartmentId.HasValue)
            {
                resolvedFilters["Кафедра"] = filterSet.DepartmentId.Value.ToString();
            }

            if (filterSet.SpecialityId.HasValue)
            {
                resolvedFilters["Специальность"] = filterSet.SpecialityId.Value.ToString();
            }

            if (filterSet.SpecializationId.HasValue)
            {
                resolvedFilters["Специализация"] = filterSet.SpecializationId.Value.ToString();
            }
        }

        return resolvedFilters;
    }

    public static async Task<Dictionary<Guid, string>> ResolveDepartmentNamesAsync(
        IEnumerable<Guid> departmentIds,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Guid> ids = departmentIds.Distinct();

        return await dbContext.Departments
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .Select(d => new { d.Id, d.Name })
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);
    }

    public static async Task<Dictionary<Guid, string>> ResolveDisciplineNamesAsync(
        IEnumerable<Guid> disciplineIds,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Guid> ids = disciplineIds.Distinct();

        return await dbContext.Disciplines
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .Select(d => new { d.Id, d.Name })
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);
    }

    public static async Task<Dictionary<Guid, string>> ResolveSpecialityNamesAsync(
        IEnumerable<Guid> specialityIds,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Guid> ids = specialityIds.Distinct();

        return await dbContext.Specialities
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
    }

    public static async Task<Dictionary<Guid, string>> ResolveSpecializationNamesAsync(
        IEnumerable<Guid> specializationIds,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Guid> ids = specializationIds.Distinct();

        return await dbContext.Specializations
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
    }

    public static async Task<Dictionary<Guid, string>> ResolveTeacherNamesAsync(
        IEnumerable<Guid> teacherIds,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Guid> ids = teacherIds.Distinct();

        return await dbContext.Teachers
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .Select(t => new { t.Id, t.FullName })
            .ToDictionaryAsync(t => t.Id, t => t.FullName, cancellationToken);
    }
}
