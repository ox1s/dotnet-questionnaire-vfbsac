using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetStaff;

internal sealed class GetStaffQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetStaffQuery, List<GetStaffQueryResponse>>
{
    public async Task<Result<List<GetStaffQueryResponse>>> Handle(GetStaffQuery query, CancellationToken cancellationToken)
    {
        List<GetStaffQueryResponse> staff = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Staff)
            .OrderBy(u => u.DisplayName)
            .Select(u => new GetStaffQueryResponse(
                u.Id,
                u.Login.Value,
                u.DisplayName))
            .ToListAsync(cancellationToken);

        return staff;
    }
}
