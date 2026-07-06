using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetGroups;

internal sealed class GetGroupsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetGroupsQuery, List<GetGroupsQueryResponse>>
{
    public async Task<Result<List<GetGroupsQueryResponse>>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        List<GetGroupsQueryResponse> groups = await context.Users
            .AsNoTracking() 
            .Where(u => u.Role == UserRole.StudentGroup)
            .OrderBy(u => u.Login.Value)
            .Select(u => new GetGroupsQueryResponse(
                u.Id,
                u.Login.Value,
                u.DisplayName))
            .ToListAsync(cancellationToken);

        return groups;
    }
}
