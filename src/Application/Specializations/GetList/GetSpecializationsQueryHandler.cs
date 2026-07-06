using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.GetList;

internal sealed class GetSpecializationsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSpecializationsQuery, List<GetSpecializationsQueryResponse>>
{
    public async Task<Result<List<GetSpecializationsQueryResponse>>> Handle(
        GetSpecializationsQuery query,
        CancellationToken cancellationToken)
    {
        List<GetSpecializationsQueryResponse> specializations = await context.Specializations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(s => s.IsDeleted)
            .ThenBy(s => s.Name)
            .Select(s => new GetSpecializationsQueryResponse(s.Id, s.Name, s.SpecialityId, s.IsDeleted))
            .ToListAsync(cancellationToken);

        return specializations;
    }
}
