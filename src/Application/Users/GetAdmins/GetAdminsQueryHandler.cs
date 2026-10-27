using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetAdmins;

internal sealed class GetAdminsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetAdminsQuery, List<GetAdminsQueryResponse>>
{
    public async Task<Result<List<GetAdminsQueryResponse>>> Handle(GetAdminsQuery query, CancellationToken cancellationToken)
    {
        List<GetAdminsQueryResponse> admins = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Admin)
            .OrderBy(u => u.Login.Value)
            .Select(u => new GetAdminsQueryResponse(
                u.Id,
                u.Login.Value,
                u.DisplayName))
            .ToListAsync(cancellationToken);

        return admins;
    }
}
