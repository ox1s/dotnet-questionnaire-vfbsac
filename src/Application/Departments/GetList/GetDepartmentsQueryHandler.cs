using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.GetList;

internal sealed class GetDepartmentsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDepartmentsQuery, List<DepartmentResponse>>
{
    public async Task<Result<List<DepartmentResponse>>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        List<DepartmentResponse> departments = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentResponse
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToListAsync(cancellationToken);

        return departments;
    }
}
