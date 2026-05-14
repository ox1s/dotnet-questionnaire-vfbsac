using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specialities.GetList;

internal sealed class GetSpecialitiesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSpecialitiesQuery, List<GetSpecialitiesQueryResponse>>
{
    public async Task<Result<List<GetSpecialitiesQueryResponse>>> Handle(
        GetSpecialitiesQuery query,
        CancellationToken cancellationToken)
    {
        List<GetSpecialitiesQueryResponse> specialities = await context.Specialities
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(s => s.IsDeleted)
            .ThenBy(s => s.Name)
            .Select(s => new GetSpecialitiesQueryResponse(s.Id, s.Name, s.IsDeleted))
            .ToListAsync(cancellationToken);

        return specialities;
    }
}
