using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Disciplines;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Disciplines.Restore;

internal sealed class RestoreDisciplineCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RestoreDisciplineCommand>
{
    public async Task<Result> Handle(RestoreDisciplineCommand command, CancellationToken cancellationToken)
    {
        Discipline? discipline = await context.Disciplines
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.DisciplineId, cancellationToken);

        if (discipline is null)
        {
            return Result.Failure(DisciplineErrors.NotFound(command.DisciplineId));
        }

        bool departmentExists = await context.Departments
            .AnyAsync(d => d.Id == discipline.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            return Result.Failure(DisciplineErrors.DepartmentDeleted(discipline.DepartmentId));
        }

        discipline.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
