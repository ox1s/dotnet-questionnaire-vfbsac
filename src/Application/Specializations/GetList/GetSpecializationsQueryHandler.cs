using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.GetList;

internal sealed class GetSpecializationsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSpecializationsQuery, List<SpecializationResponse>>
{
    public async Task<Result<List<SpecializationResponse>>> Handle(
        GetSpecializationsQuery query,
        CancellationToken cancellationToken)
    {
        List<SpecializationResponse> specializations = await context.Specializations
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecializationResponse(s.Id, s.Name, s.SpecialityId))
            .ToListAsync(cancellationToken);

        return specializations;
    }
}
