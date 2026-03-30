using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Submissions.Create;

internal sealed class CreateSubmissionCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateSubmissionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSubmissionCommand command, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);
        
        if (form is null)
        {
            return Result.Failure<Guid>(FormErrors.NotFound(command.FormId));
        }

        if (!form.IsActive)
        {
            return Result.Failure<Guid>(FormErrors.FormInactive(command.FormId));
        }

        if (form.IsDeleted)
        {
            return Result.Failure<Guid>(FormErrors.NotFound(command.FormId));
        }

        IQueryable<Submission> query = context.Submissions
            .Where(s => s.FormId == command.FormId &&
                s.UserId == command.UserId &&
                s.DeviceId == command.DeviceId);

        if (command.TeacherId.HasValue)
        {
            query = query.Where(s => s.Context.TeacherId == command.TeacherId);
        }

        if (command.DisciplineId.HasValue)
        {
            query = query.Where(s => s.Context.DisciplineId == command.DisciplineId);
        }

        bool alreadyExists = await query.AnyAsync(cancellationToken);

        if (alreadyExists)
        {
            return Result.Failure<Guid>(SubmissionErrors.AlreadySubmitted());
        }

        Result<Submission> submissionResult = Submission.Create(
            command.FormId,
            command.DeviceId,
            command.UserId,
            command.DisciplineId,
            command.TeacherId,
            command.DepartmentId,
            command.SpecialityId,
            command.SpecializationId,
            command.OrganizationName);

        if (submissionResult.IsFailure)
        {
            return Result.Failure<Guid>(submissionResult.Error);
        }

        Submission submission = submissionResult.Value;

        SubmissionContext newContext = submission.Context with
        {
            EducationForm = command.EducationForm,
            EmployeeCategory = command.EmployeeCategory,
            Position = command.Position
        };
        submission.UpdateContext(newContext);

        foreach (AnswerRequest answerRequest in command.Answers)
        {
            Result<Answer> answerResult = submission.AddAnswer(
                answerRequest.QuestionId,
                answerRequest.Value,
                answerRequest.NumericValue,
                answerRequest.Weight);

            if (answerResult.IsFailure)
            {
                return Result.Failure<Guid>(answerResult.Error);
            }
        }

        context.Submissions.Add(submission);
        await context.SaveChangesAsync(cancellationToken);

        return submission.Id;
    }
}
