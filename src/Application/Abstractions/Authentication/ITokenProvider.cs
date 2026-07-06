using Domain.User;

namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);
}
