using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Delete;

internal sealed class DeleteUserCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
