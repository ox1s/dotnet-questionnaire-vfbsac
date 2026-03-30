using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.College.Teachers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Teachers.Restore;

internal sealed class RestoreTeacherCommandHandler(IApplicationDbContext context)
    : ICommandHandler<RestoreTeacherCommand>
{
    public async Task<Result> Handle(RestoreTeacherCommand command, CancellationToken cancellationToken)
    {
        Teacher? teacher = await context.Teachers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == command.TeacherId, cancellationToken);

        if (teacher is null)
        {
            return Result.Failure(TeacherErrors.NotFound(command.TeacherId));
        }

        teacher.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
