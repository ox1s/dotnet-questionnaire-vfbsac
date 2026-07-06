using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Disciplines;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Disciplines.Delete;

internal sealed class DeleteDisciplineCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteDisciplineCommand>
{
    public async Task<Result> Handle(DeleteDisciplineCommand command, CancellationToken cancellationToken)
    {
        Discipline? discipline = await context.Disciplines
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.DisciplineId, cancellationToken);

        if (discipline is null)
        {
            return Result.Failure(DisciplineErrors.NotFound(command.DisciplineId));
        }

        bool usedInSubmissions = await context.Submissions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Context.DisciplineId == command.DisciplineId, cancellationToken);

        if (usedInSubmissions)
        {
            discipline.IsDeleted = true;
            context.Disciplines.Update(discipline);
        }
        else
        {
            context.Disciplines.Remove(discipline);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
