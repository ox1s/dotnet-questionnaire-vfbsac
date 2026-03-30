using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetGroups;

internal sealed class GetGroupsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetGroupsQuery, List<GroupResponse>>
{
    public async Task<Result<List<GroupResponse>>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        List<GroupResponse> groups = await context.Users
            .AsNoTracking() 
            .Where(u => u.Role == UserRole.StudentGroup)
            .OrderBy(u => u.Login.Value)
            .Select(u => new GroupResponse(
                u.Id,
                u.Login.Value,
                u.DisplayName))
            .ToListAsync(cancellationToken);

        return groups;
    }
}
