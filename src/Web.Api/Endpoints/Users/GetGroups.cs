using Application.Abstractions.Data;
using Domain.UserAggregate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Users;

// Для простоты делаем Query прямо здесь (Minimal API style), 
// но по правилам Clean Arch лучше вынести в Application слой (GetUsersByRoleQuery).
// Сделаем быстро:
internal sealed class GetGroups : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/groups", async (
            [FromServices] IApplicationDbContext context,
            CancellationToken ct) =>
        {
            // Получаем всех юзеров с ролью StudentGroup (enum = 3)
            var groups = await context.Users
                .Where(u => u.Role == UserRole.StudentGroup)
                .Select(u => new 
                { 
                    u.Id, 
                    Login = u.Login.Value, // Название группы
                    u.DisplayName 
                })
                .OrderBy(u => u.Login)
                .ToListAsync(ct);

            return Results.Ok(groups);
        })
        .WithTags("Users")
        .RequireAuthorization();
    }
}
