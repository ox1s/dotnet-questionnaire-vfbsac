using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.AdminSetPassword;

internal sealed class AdminSetPasswordCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher) : ICommandHandler<AdminSetPasswordCommand>
{
    public async Task<Result> Handle(AdminSetPasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        string hash = passwordHasher.Hash(command.NewPassword);
        user.SetPasswordByAdmin(hash);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
