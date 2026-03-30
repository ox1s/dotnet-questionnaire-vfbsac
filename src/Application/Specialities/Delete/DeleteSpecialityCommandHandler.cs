using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specialities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specialities.Delete;

internal sealed class DeleteSpecialityCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteSpecialityCommand>
{
    public async Task<Result> Handle(DeleteSpecialityCommand command, CancellationToken cancellationToken)
    {
        Speciality? speciality = await context.Specialities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == command.SpecialityId, cancellationToken);

        if (speciality is null)
        {
            return Result.Failure(SpecialityErrors.NotFound(command.SpecialityId));
        }

        bool hasSpecializations = await context.Specializations
            .AnyAsync(s => s.SpecialityId == command.SpecialityId, cancellationToken);

        if (hasSpecializations)
        {
            return Result.Failure(SpecialityErrors.HasSpecializations());
        }

        bool usedInSubmissions = await context.Submissions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Context.SpecialityId == command.SpecialityId, cancellationToken);

        if (usedInSubmissions)
        {
            speciality.IsDeleted = true;
            context.Specialities.Update(speciality);
        }
        else
        {
            context.Specialities.Remove(speciality);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
