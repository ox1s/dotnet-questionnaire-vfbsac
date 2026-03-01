using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.SubmissionAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Submissions.Delete;

internal sealed class DeleteSubmissionCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteSubmissionCommand>
{
    public async Task<Result> Handle(DeleteSubmissionCommand command, CancellationToken cancellationToken)
    {
        Submission? submission = await context.Submissions
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure(SubmissionErrors.NotFound(command.SubmissionId));
        }

        submission.MarkAsDeleted();
        context.Submissions.Update(submission);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
