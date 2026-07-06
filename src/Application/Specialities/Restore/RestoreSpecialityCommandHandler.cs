using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specialities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specialities.Restore;

internal sealed class RestoreSpecialityCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RestoreSpecialityCommand>
{
    public async Task<Result> Handle(RestoreSpecialityCommand command, CancellationToken cancellationToken)
    {
        Speciality? speciality = await context.Specialities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == command.SpecialityId, cancellationToken);

        if (speciality is null)
        {
            return Result.Failure(SpecialityErrors.NotFound(command.SpecialityId));
        }

        speciality.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
