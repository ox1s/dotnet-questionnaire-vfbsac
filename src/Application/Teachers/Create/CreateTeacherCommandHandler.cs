using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Departments;
using Domain.College.Teachers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Create;

internal sealed class CreateTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateTeacherCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeacherCommand command, CancellationToken cancellationToken)
    {
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
                return Result.Failure<Guid>(DepartmentErrors.NotFound(missingIds[0]));
            }
        }

        Result<Teacher> teacherResult = Teacher.Create(command.FullName, departmentIds);

        if (teacherResult.IsFailure)
        {
            return Result.Failure<Guid>(teacherResult.Error);
        }

        context.Teachers.Add(teacherResult.Value);
        await context.SaveChangesAsync(cancellationToken);

        return teacherResult.Value.Id;
    }
}
