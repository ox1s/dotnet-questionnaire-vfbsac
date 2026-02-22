// src/Infrastructure/Authorization/PermissionAuthorizationHandler.cs
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 1. Проверяем, что пользователь аутентифицирован
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return Task.CompletedTask;
        }

        // 2. Ищем нужный claim "permission" в токене
        if (context.User.HasClaim("permission", requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
