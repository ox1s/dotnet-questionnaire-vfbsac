using System.Security.Claims;
using Domain.User;

namespace Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("User id is unavailable");
    }

    public static UserRole GetRole(this ClaimsPrincipal? principal)
    {
        string? role = principal?.FindFirstValue("role");

        return Enum.TryParse(role, out UserRole parsedRole) ?
            parsedRole :
            throw new ApplicationException("User role is unavailable");
    }
}
