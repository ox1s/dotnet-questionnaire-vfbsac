using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Departments.Delete;

internal sealed class DeleteDepartmentCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteDepartmentCommand>
{
    public async Task<Result> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        Department? department = await context.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == command.DepartmentId, cancellationToken);

        if (department is null)
        {
            return Result.Failure(DepartmentErrors.NotFound(command.DepartmentId));
        }

        bool hasTeachers = await context.Teachers
            .AnyAsync(t => t.DepartmentId == command.DepartmentId, cancellationToken);

        if (hasTeachers)
        {
            return Result.Failure(DepartmentErrors.HasTeachers());
        }

        bool hasDisciplines = await context.Disciplines
            .AnyAsync(d => d.DepartmentId == command.DepartmentId, cancellationToken);

        if (hasDisciplines)
        {
            return Result.Failure(DepartmentErrors.HasDisciplines());
        }

        bool hasUsers = await context.Users
            .AnyAsync(u => u.DepartmentId == command.DepartmentId, cancellationToken);

        if (hasUsers)
        {
            return Result.Failure(DepartmentErrors.HasUsers());
        }

        bool usedInSubmissions = await context.Submissions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Context.DepartmentId == command.DepartmentId, cancellationToken);

        if (usedInSubmissions)
        {
            department.IsDeleted = true;
            context.Departments.Update(department);
        }
        else
        {
            context.Departments.Remove(department);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
