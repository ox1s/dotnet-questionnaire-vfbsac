using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Domain.College.Disciplines;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Disciplines.Create;

internal sealed class CreateDisciplineCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateDisciplineCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDisciplineCommand command, CancellationToken cancellationToken)
    {
        bool departmentExists = await context.Departments
            .AnyAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (!departmentExists)
        {
            return Result.Failure<Guid>(DepartmentErrors.NotFound(command.DepartmentId));
        }
        
        Result<Discipline> disciplineResult = Discipline.Create(command.Name, command.DepartmentId);

        if (disciplineResult.IsFailure)
        {
            return Result.Failure<Guid>(disciplineResult.Error);
        }
        
        context.Disciplines.Add(disciplineResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return disciplineResult.Value.Id;
    }
}
