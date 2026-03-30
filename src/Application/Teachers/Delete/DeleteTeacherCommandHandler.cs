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
            .FirstOrDefaultAsync(t => t.Id == command.TeacherId, cancellationToken);

        if (teacher is null)
        {
            return Result.Failure(TeacherErrors.NotFound(command.TeacherId));
        }

        teacher.IsDeleted = true;
        context.Teachers.Update(teacher);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
