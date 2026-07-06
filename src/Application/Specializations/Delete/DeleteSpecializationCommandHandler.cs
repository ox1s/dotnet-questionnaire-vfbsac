using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Specializations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Specializations.Delete;

internal sealed class DeleteSpecializationCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteSpecializationCommand>
{
    public async Task<Result> Handle(DeleteSpecializationCommand command, CancellationToken cancellationToken)
    {
        Specialization? specialization = await context.Specializations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == command.SpecializationId, cancellationToken);

        if (specialization is null)
        {
            return Result.Failure(SpecializationErrors.NotFound(command.SpecializationId));
        }

        bool usedInSubmissions = await context.Submissions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Context.SpecializationId == command.SpecializationId, cancellationToken);

        if (usedInSubmissions)
        {
            specialization.IsDeleted = true;
            context.Specializations.Update(specialization);
        }
        else
        {
            context.Specializations.Remove(specialization);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
