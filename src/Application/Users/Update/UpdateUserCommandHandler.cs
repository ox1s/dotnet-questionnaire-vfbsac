using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Update;

internal sealed class UpdateUserCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        Result<Domain.UserAggregate.Login> loginResult = Domain.UserAggregate.Login.Create(command.Login);
        if (loginResult.IsFailure)
        {
            return Result.Failure(loginResult.Error);
        }

        if (user.Login.Value != loginResult.Value.Value)
        {
            bool exists = await context.Users
                .AnyAsync(u => u.Login.Value == loginResult.Value.Value, cancellationToken);

            if (exists)
            {
                return Result.Failure(UserErrors.Conflict("Users.Duplicate", "Логин занят"));
            }
        }

        user.UpdateDetails(loginResult.Value, command.DisplayName);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
