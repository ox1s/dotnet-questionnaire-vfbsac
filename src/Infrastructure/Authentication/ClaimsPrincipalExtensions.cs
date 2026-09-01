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
        // JwtBearerHandler's default inbound claim mapping (MapInboundClaims = true,
        // never overridden in this project) renames the token's "role" claim to
        // ClaimTypes.Role by the time it reaches the validated principal. Falling
        // back to the raw "role" key keeps this working if that mapping is ever
        // turned off.
        string? role = principal?.FindFirstValue(ClaimTypes.Role) ?? principal?.FindFirstValue("role");

        return Enum.TryParse(role, out UserRole parsedRole) && Enum.IsDefined(parsedRole) ?
            parsedRole :
            throw new ApplicationException("User role is unavailable");
    }
}
