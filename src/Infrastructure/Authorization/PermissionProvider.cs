using Application.Abstractions.Data;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        User? user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return [];
        }

        HashSet<string> permissions = [];

        switch (user.Role)
        {
            case UserRole.Admin:
                permissions.Add(Permissions.Admin);
                permissions.Add(Permissions.UsersAccess);
                permissions.Add(Permissions.DictionariesWrite);
                permissions.Add(Permissions.ReportsView);
                break;
            case UserRole.StudentGroup:
                permissions.Add(Permissions.SubmitForms);
                break;
            case UserRole.Staff:
            case UserRole.DeputyHead:
                permissions.Add(Permissions.ReportsView);
                break;
        }
        return permissions;
    }
}
