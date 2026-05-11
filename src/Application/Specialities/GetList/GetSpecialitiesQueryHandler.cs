using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specialities.GetList;

internal sealed class GetSpecialitiesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSpecialitiesQuery, List<GetSpecialityQueryResponse>>
{
    public async Task<Result<List<GetSpecialityQueryResponse>>> Handle(
        GetSpecialitiesQuery query,
        CancellationToken cancellationToken)
    {
        List<GetSpecialityQueryResponse> specialities = await context.Specialities
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(s => s.IsDeleted)
            .ThenBy(s => s.Name)
            .Select(s => new GetSpecialityQueryResponse(s.Id, s.Name, s.IsDeleted))
            .ToListAsync(cancellationToken);

        return specialities;
    }
}
