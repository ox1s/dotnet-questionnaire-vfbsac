using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specialities;
using Domain.College.Specializations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.Update;

internal sealed class UpdateSpecializationCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateSpecializationCommand>
{
    public async Task<Result> Handle(UpdateSpecializationCommand command, CancellationToken cancellationToken)
    {
        Specialization? specialization = await context.Specializations
            .FirstOrDefaultAsync(s => s.Id == command.SpecializationId, cancellationToken);

        if (specialization is null)
        {
            return Result.Failure(SpecializationErrors.NotFound(command.SpecializationId));
        }

        Specialization? specializationWithSameName = await context.Specializations
            .FirstOrDefaultAsync(s => s.Name == command.Name && s.Id != command.SpecializationId, cancellationToken);

        if (specializationWithSameName is not null)
        {
            return Result.Failure(SpecializationErrors.Duplicate(command.Name));
        }

        Result updateNameResult = specialization.UpdateName(command.Name);
        if (updateNameResult.IsFailure)
        {
            return Result.Failure(updateNameResult.Error);
        }

        if (specialization.SpecialityId != command.SpecialityId)
        {
            bool specialityExists = await context.Specialities
                .AnyAsync(s => s.Id == command.SpecialityId, cancellationToken);

            if (!specialityExists)
            {
                return Result.Failure(SpecialityErrors.NotFound(command.SpecialityId));
            }

            specialization.ChangeSpeciality(command.SpecialityId);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
