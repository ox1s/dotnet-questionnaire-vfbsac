using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.DepartmentAggregate;
using Domain.College.TeacherAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Update;

internal sealed class UpdateTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateTeacherCommand>
{
    public async Task<Result> Handle(UpdateTeacherCommand command, CancellationToken cancellationToken)
    {
        Teacher? teacher = await context.Teachers
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

        if (teacher.DepartmentId != command.DepartmentId)
        {
            bool deptExists = await context.Departments
                .AnyAsync(d => d.Id == command.DepartmentId, cancellationToken);

            if (!deptExists)
            {
                return Result.Failure(DepartmentErrors.NotFound(command.DepartmentId));
            }

            teacher.ChangeDepartment(command.DepartmentId);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
