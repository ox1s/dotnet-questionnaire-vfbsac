using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.OpenSemester;

public class OpenSemesterCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<OpenSemesterCommand>
{
    public async Task<Result> Handle(OpenSemesterCommand command, CancellationToken cancellationToken)
    {
        await context.Forms
             .Where(f => !f.IsActive)
             .ExecuteUpdateAsync(s =>
                s.SetProperty(f => f.IsActive, true), cancellationToken);

        return Result.Success();
    }
}
