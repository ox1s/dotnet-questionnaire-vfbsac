using Application.Abstractions.Authentication;
using Domain.User;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new ApplicationException("User context is unavailable");

    public UserRole Role =>
        httpContextAccessor
            .HttpContext?
            .User
            .GetRole() ??
        throw new ApplicationException("User context is unavailable");
}
