using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.DepartmentAggregate;
using Domain.College.DisciplineAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Disciplines.Update;

internal sealed class UpdateDisciplineCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateDisciplineCommand>
{
    public async Task<Result> Handle(UpdateDisciplineCommand command, CancellationToken cancellationToken)
    {
        Discipline? discipline = await context.Disciplines
            .FirstOrDefaultAsync(d => d.Id == command.DisciplineId, cancellationToken);

        if (discipline is null)
        {
            return Result.Failure(DisciplineErrors.NotFound(command.DisciplineId));
        }

        discipline.UpdateName(command.Name);

        if (discipline.DepartmentId != command.DepartmentId)
        {
             bool deptExists = await context.Departments
                 .AnyAsync(d => d.Id == command.DepartmentId, cancellationToken);
             
             if (!deptExists)
             {
                 return Result.Failure(DepartmentErrors.NotFound(command.DepartmentId));
             }
             discipline.ChangeDepartment(command.DepartmentId);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
