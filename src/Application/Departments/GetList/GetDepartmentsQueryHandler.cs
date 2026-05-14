using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.GetList;

internal sealed class GetDepartmentsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDepartmentsQuery, List<GetDepartmentsQueryResponse>>
{
    public async Task<Result<List<GetDepartmentsQueryResponse>>> Handle(GetDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        List<GetDepartmentsQueryResponse> departments = await context.Departments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(d => d.IsDeleted)
            .ThenBy(d => d.Name)
            .Select(d => new GetDepartmentsQueryResponse(d.Id, d.Name, d.IsDeleted))
            .ToListAsync(cancellationToken);

        return departments;
    }
}
