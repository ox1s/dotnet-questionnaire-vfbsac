using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.GetList;

internal sealed class GetDepartmentsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDepartmentsQuery, List<GetDepartmentQueryResponse>>
{
    public async Task<Result<List<GetDepartmentQueryResponse>>> Handle(GetDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        List<GetDepartmentQueryResponse> departments = await context.Departments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(d => d.IsDeleted)
            .ThenBy(d => d.Name)
            .Select(d => new GetDepartmentQueryResponse(d.Id, d.Name, d.IsDeleted))
            .ToListAsync(cancellationToken);

        return departments;
    }
}
