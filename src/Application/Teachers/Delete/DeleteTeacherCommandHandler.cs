using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Teachers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Delete;

internal sealed class DeleteTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteTeacherCommand>
{
    public async Task<Result> Handle(DeleteTeacherCommand command, CancellationToken cancellationToken)
    {
        Teacher? teacher = await context.Teachers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == command.TeacherId, cancellationToken);

        if (teacher is null)
        {
            return Result.Failure(TeacherErrors.NotFound(command.TeacherId));
        }

        bool hasUsers = await context.Users
            .AnyAsync(u => u.TeacherId == command.TeacherId, cancellationToken);

        if (hasUsers)
        {
            return Result.Failure(TeacherErrors.HasUsers());
        }

        bool usedInSubmissions = await context.Submissions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Context.TeacherId == command.TeacherId, cancellationToken);

        if (usedInSubmissions)
        {
            teacher.IsDeleted = true;
            context.Teachers.Update(teacher);
        }
        else
        {
            context.Teachers.Remove(teacher);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
