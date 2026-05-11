using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.GetList;

internal sealed class GetSpecializationsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSpecializationsQuery, List<GetSpecializationQueryResponse>>
{
    public async Task<Result<List<GetSpecializationQueryResponse>>> Handle(
        GetSpecializationsQuery query,
        CancellationToken cancellationToken)
    {
        List<GetSpecializationQueryResponse> specializations = await context.Specializations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(s => s.IsDeleted)
            .ThenBy(s => s.Name)
            .Select(s => new GetSpecializationQueryResponse(s.Id, s.Name, s.SpecialityId, s.IsDeleted))
            .ToListAsync(cancellationToken);

        return specializations;
    }
}
