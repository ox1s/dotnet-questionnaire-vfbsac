using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Application.Disciplines.GetList;

public class GetDisciplinesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDisciplinesQuery, List<GetDisciplinesQueryResponse>>
{
    public async Task<Result<List<GetDisciplinesQueryResponse>>> Handle(GetDisciplinesQuery query, CancellationToken cancellationToken)
    {
        List<GetDisciplinesQueryResponse> disciplines = await context.Disciplines
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(d => d.IsDeleted)
            .ThenBy(d => d.Name)
            .Select(d => new GetDisciplinesQueryResponse(d.Id, d.Name, d.DepartmentId, d.IsDeleted))
            .ToListAsync(cancellationToken);

        return disciplines;
    }
}
