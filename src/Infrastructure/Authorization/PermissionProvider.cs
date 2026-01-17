using Application.Abstractions.Data;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // 1. Получаем пользователя и его роль
        User? user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return [];
        }

        // 2. Назначаем права в зависимости от роли
        HashSet<string> permissions = [];

        switch (user.Role)
        {
            case UserRole.Admin:
                permissions.Add("admin:access");      // Доступ к админке
                permissions.Add("users:access");      // Управление юзерами
                permissions.Add("dictionaries:write");// Редактирование справочников
                permissions.Add("reports:view");      // Просмотр отчетов
                break;

            case UserRole.StudentGroup:
                permissions.Add("forms:submit");      // Прохождение анкет
                break;

            case UserRole.Staff:
            case UserRole.DeputyHead:
                permissions.Add("reports:view");      // Просмотр отчетов (своей кафедры)
                break;
        }

        return permissions;
    }
}
