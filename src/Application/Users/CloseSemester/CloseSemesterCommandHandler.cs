using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.CloseSemester;

public class CloseSemesterCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<CloseSemesterCommand>
{
    public async Task<Result> Handle(CloseSemesterCommand command, CancellationToken cancellationToken)
    {
        await context.Forms
             .Where(f => f.IsActive)
             .ExecuteUpdateAsync(s =>
                s.SetProperty(f => f.IsActive, false), cancellationToken);

        return Result.Success();
    }

}
