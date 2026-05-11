using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.GetList;

internal sealed class GetTeachersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTeachersQuery, List<GetTeacherQueryResponse>>
{
    public async Task<Result<List<GetTeacherQueryResponse>>> Handle(GetTeachersQuery query, CancellationToken cancellationToken)
    {
        List<GetTeacherQueryResponse> teachers = await context.Teachers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.IsDeleted)
            .ThenBy(t => t.FullName)
            .Select(t => new GetTeacherQueryResponse(t.Id, t.FullName, t.DepartmentId, t.IsDeleted))
            .ToListAsync(cancellationToken);

        return teachers;
    }
}
