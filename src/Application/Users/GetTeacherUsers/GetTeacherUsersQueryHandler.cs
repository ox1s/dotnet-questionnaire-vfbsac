using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetTeacherUsers;

internal sealed class GetTeacherUsersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetTeacherUsersQuery, List<GetTeacherUsersQueryResponse>>
{
    public async Task<Result<List<GetTeacherUsersQueryResponse>>> Handle(GetTeacherUsersQuery query, CancellationToken cancellationToken)
    {
        List<GetTeacherUsersQueryResponse> teachers = await context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Teacher && u.TeacherId != null)
            .OrderBy(u => u.DisplayName)
            .Select(u => new GetTeacherUsersQueryResponse(
                u.Id,
                u.Login.Value,
                u.DisplayName,
                u.TeacherId!.Value))
            .ToListAsync(cancellationToken);

        return teachers;
    }
}
