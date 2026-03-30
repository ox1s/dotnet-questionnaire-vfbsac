using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specialities.GetList;

internal sealed class GetSpecialitiesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSpecialitiesQuery, List<SpecialityResponse>>
{
    public async Task<Result<List<SpecialityResponse>>> Handle(
        GetSpecialitiesQuery query,
        CancellationToken cancellationToken)
    {
        List<SpecialityResponse> specialities = await context.Specialities
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecialityResponse(s.Id, s.Name))
            .ToListAsync(cancellationToken);

        return specialities;
    }
}
