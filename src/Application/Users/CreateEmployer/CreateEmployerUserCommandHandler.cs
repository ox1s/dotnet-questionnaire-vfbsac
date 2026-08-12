using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.CreateEmployer;

internal sealed class CreateEmployerUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher)
    : ICommandHandler<CreateEmployerUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEmployerUserCommand command, CancellationToken cancellationToken)
    {
        Result<Login> loginResult = Login.Create(command.Login);
        if (loginResult.IsFailure)
        {
            return Result.Failure<Guid>(loginResult.Error);
        }

        if (await context.Users.AnyAsync(u => u.Login.Value == loginResult.Value.Value, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.UserExist());
        }

        string hash = passwordHasher.Hash(command.Password);

        Result<User> userResult = User.CreateEmployer(
            loginResult.Value,
            command.DisplayName,
            command.OrganizationName,
            hash
        );

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        context.Users.Add(userResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return userResult.Value.Id;
    }
}
