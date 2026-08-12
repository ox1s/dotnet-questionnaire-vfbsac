using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Teachers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.GetList;

internal sealed class GetTeachersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTeachersQuery, List<GetTeachersQueryResponse>>
{
    public async Task<Result<List<GetTeachersQueryResponse>>> Handle(GetTeachersQuery query, CancellationToken cancellationToken)
    {
        List<Teacher> teachers = await context.Teachers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.IsDeleted)
            .ThenBy(t => t.FullName)
            .ToListAsync(cancellationToken);

        var teacherIds = teachers.Select(t => t.Id).ToList();

        List<TeacherDepartment> teacherDepartments = await context.TeacherDepartments
            .AsNoTracking()
            .Where(td => teacherIds.Contains(td.TeacherId))
            .ToListAsync(cancellationToken);

        var departmentIdsByTeacher = teacherDepartments
            .GroupBy(td => td.TeacherId)
            .ToDictionary(g => g.Key, g => g.Select(td => td.DepartmentId).ToList());

        return teachers
            .Select(t => new GetTeachersQueryResponse(
                t.Id,
                t.FullName,
                departmentIdsByTeacher.TryGetValue(t.Id, out List<Guid>? ids) ? ids : [],
                t.IsDeleted))
            .ToList();
    }
}
