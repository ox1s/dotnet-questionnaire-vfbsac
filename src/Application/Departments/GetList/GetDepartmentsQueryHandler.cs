using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.GetList;

internal sealed class GetDepartmentsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDepartmentsQuery, List<GetDepartmentResponse>>
{
    public async Task<Result<List<GetDepartmentResponse>>> Handle(GetDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        List<GetDepartmentResponse> departments = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new GetDepartmentResponse(d.Id, d.Name))
            .ToListAsync(cancellationToken);

        return departments;
    }
}
