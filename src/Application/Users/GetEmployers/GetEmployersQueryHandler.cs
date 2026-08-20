using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetEmployers;

internal sealed class GetEmployersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetEmployersQuery, List<GetEmployersQueryResponse>>
{
    public async Task<Result<List<GetEmployersQueryResponse>>> Handle(GetEmployersQuery query, CancellationToken cancellationToken)
    {
        List<GetEmployersQueryResponse> employers = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Employer)
            .OrderBy(u => u.DisplayName)
            .Select(u => new GetEmployersQueryResponse(
                u.Id,
                u.Login.Value,
                u.DisplayName,
                u.OrganizationName))
            .ToListAsync(cancellationToken);

        return employers;
    }
}
