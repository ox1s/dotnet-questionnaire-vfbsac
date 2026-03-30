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
            .FirstOrDefaultAsync(s => s.Id == command.SpecialityId, cancellationToken);

        if (speciality is null)
        {
            return Result.Failure(SpecialityErrors.NotFound(command.SpecialityId));
        }

        speciality.IsDeleted = true;
        context.Specialities.Update(speciality);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
