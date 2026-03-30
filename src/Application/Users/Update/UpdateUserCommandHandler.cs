using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.User;
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

        Result<Login> loginResult = Login.Create(command.Login);
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
                return Result.Failure(UserErrors.UserExist());
            }
        }

        Result updateUserDetailsResult = user.UpdateDetails(loginResult.Value, command.DisplayName);
        if (updateUserDetailsResult.IsFailure)
        {
            return Result.Failure(updateUserDetailsResult.Error);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
