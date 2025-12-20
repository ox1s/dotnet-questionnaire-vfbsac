using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        Result<Domain.UserAggregate.Login> loginResult = Domain.UserAggregate.Login.Create(command.Login);
        if (loginResult.IsFailure)
        {
            return Result.Failure<string>(UserErrors.NotFoundByLogin(command.Login));
        }

        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Login.Value == loginResult.Value.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFoundByLogin(command.Login));
        }

        // 3. Проверяем пароль
        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure<string>(UserErrors.NotFoundByLogin(command.Login)); // Или "InvalidPassword"
        }

        string token = tokenProvider.Create(user);

        return token;
    }
}
