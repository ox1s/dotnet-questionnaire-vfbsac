using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specialities;
using Domain.College.Specializations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.Create;

internal sealed class CreateSpecializationCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateSpecializationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSpecializationCommand command, CancellationToken cancellationToken)
    {
        bool specialityExists = await context.Specialities
            .AnyAsync(s => s.Id == command.SpecialityId, cancellationToken);

        if (!specialityExists)
        {
            return Result.Failure<Guid>(SpecialityErrors.NotFound(command.SpecialityId));
        }

        Result<Specialization> specializationResult = Specialization.Create(command.Name, command.SpecialityId);
        if (specializationResult.IsFailure)
        {
            return Result.Failure<Guid>(specializationResult.Error);
        }

        context.Specializations.Add(specializationResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return specializationResult.Value.Id;
    }
}
