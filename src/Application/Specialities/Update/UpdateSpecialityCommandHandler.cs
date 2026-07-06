using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specialities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specialities.Update;

internal sealed class UpdateSpecialityCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateSpecialityCommand>
{
    public async Task<Result> Handle(UpdateSpecialityCommand command, CancellationToken cancellationToken)
    {
        Speciality? speciality = await context.Specialities
            .FirstOrDefaultAsync(s => s.Id == command.SpecialityId, cancellationToken);

        if (speciality is null)
        {
            return Result.Failure(SpecialityErrors.NotFound(command.SpecialityId));
        }

        Speciality? specialityWithSameName = await context.Specialities
            .FirstOrDefaultAsync(s => s.Name == command.Name && s.Id != command.SpecialityId, cancellationToken);

        if (specialityWithSameName is not null)
        {
            return Result.Failure(SpecialityErrors.Duplicate(command.Name));
        }

        Result updateNameResult = speciality.UpdateName(command.Name);
        if (updateNameResult.IsFailure)
        {
            return Result.Failure(updateNameResult.Error);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
