using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Application.Disciplines.GetList;

public class GetDisciplinesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetDisciplinesQuery, List<DisciplineResponse>>
{
    public async Task<Result<List<DisciplineResponse>>> Handle(GetDisciplinesQuery query, CancellationToken cancellationToken)
    {
        List<DisciplineResponse> disciplines = await context.Disciplines
            .OrderBy(d => d.Name)
            .Select(d => new DisciplineResponse(d.Id, d.Name, d.DepartmentId))
            .ToListAsync(cancellationToken);

        return disciplines;
    }
}
