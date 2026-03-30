using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.GetList;

internal sealed class GetTeachersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTeachersQuery, List<TeacherResponse>>
{
    public async Task<Result<List<TeacherResponse>>> Handle(GetTeachersQuery query, CancellationToken cancellationToken)
    {
        List<TeacherResponse> teachers = await context.Teachers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.IsDeleted)
            .ThenBy(t => t.FullName)
            .Select(t => new TeacherResponse(t.Id, t.FullName, t.IsDeleted))
            .ToListAsync(cancellationToken);

        return teachers;
    }
}
