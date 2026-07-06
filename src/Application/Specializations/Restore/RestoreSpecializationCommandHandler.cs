using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specializations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.Restore;

internal sealed class RestoreSpecializationCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RestoreSpecializationCommand>
{
    public async Task<Result> Handle(RestoreSpecializationCommand command, CancellationToken cancellationToken)
    {
        Specialization? specialization = await context.Specializations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == command.SpecializationId, cancellationToken);

        if (specialization is null)
        {
            return Result.Failure(SpecializationErrors.NotFound(command.SpecializationId));
        }

        bool specialityExists = await context.Specialities
            .AnyAsync(s => s.Id == specialization.SpecialityId, cancellationToken);

        if (!specialityExists)
        {
            return Result.Failure(SpecializationErrors.SpecialityDeleted(specialization.SpecialityId));
        }

        specialization.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
