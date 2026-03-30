using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        Result<Domain.User.Login> loginResult = Domain.User.Login.Create(command.Login);
        if (loginResult.IsFailure)
        {
            return Result.Failure<Guid>(loginResult.Error);
        }

        if (await context.Users.AnyAsync(u => u.Login.Value == loginResult.Value.Value, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.UserExist());
        }

        Result<User> userResult = User.CreateAdmin(loginResult.Value, passwordHasher.Hash(command.Password));

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        context.Users.Add(userResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return userResult.Value.Id;
    }
}
