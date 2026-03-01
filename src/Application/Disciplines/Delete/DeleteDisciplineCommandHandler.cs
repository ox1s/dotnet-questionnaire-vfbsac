using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.DisciplineAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Disciplines.Delete;

internal sealed class DeleteDisciplineCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteDisciplineCommand>
{
    public async Task<Result> Handle(DeleteDisciplineCommand command, CancellationToken cancellationToken)
    {
        Discipline? discipline = await context.Disciplines
            .FirstOrDefaultAsync(d => d.Id == command.DisciplineId, cancellationToken);

        if (discipline is null)
        {
            return Result.Failure(DisciplineErrors.NotFound(command.DisciplineId));
        }

        discipline.MarkAsDeleted();
        context.Disciplines.Update(discipline);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
