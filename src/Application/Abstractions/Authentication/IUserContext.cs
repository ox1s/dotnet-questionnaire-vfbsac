using Domain.User;

namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }
    UserRole Role { get; }
}
