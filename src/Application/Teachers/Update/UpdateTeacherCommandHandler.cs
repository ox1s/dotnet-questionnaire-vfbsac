using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Domain.College.Teachers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Update;

internal sealed class UpdateTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateTeacherCommand>
{
    public async Task<Result> Handle(UpdateTeacherCommand command, CancellationToken cancellationToken)
    {
        Teacher? teacher = await context.Teachers
            .Include("_departments")
            .FirstOrDefaultAsync(t => t.Id == command.TeacherId, cancellationToken);
        if (teacher is null)
        {
            return Result.Failure(TeacherErrors.NotFound(command.TeacherId));
        }

        Result teacherResult = teacher.UpdateFullName(command.FullName);
        if (teacherResult.IsFailure)
        {
            return Result.Failure(teacherResult.Error);
        }

        var departmentIds = (command.DepartmentIds ?? []).Distinct().ToList();

        if (departmentIds.Count > 0)
        {
            List<Guid> existingIds = await context.Departments
                .Where(d => departmentIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync(cancellationToken);

            Guid[] missingIds = departmentIds.Except(existingIds).ToArray();
            if (missingIds.Length > 0)
            {
                return Result.Failure(DepartmentErrors.NotFound(missingIds[0]));
            }
        }

        foreach (Guid departmentId in teacher.DepartmentIds.Except(departmentIds).ToList())
        {
            teacher.RemoveDepartment(departmentId);
        }

        foreach (Guid departmentId in departmentIds.Except(teacher.DepartmentIds).ToList())
        {
            teacher.AssignDepartment(departmentId);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
