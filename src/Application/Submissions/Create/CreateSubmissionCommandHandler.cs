using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Domain.User;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Submissions.Create;

internal sealed class CreateSubmissionCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateSubmissionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSubmissionCommand command, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .Include(f => f.Questions)
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

        // Employers already have their organization on file; trust that over whatever
        // (if anything) the client sent, since the survey UI never asks them to type it.
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
        string? organizationName = user?.Role == UserRole.Employer
            ? user.OrganizationName
            : command.OrganizationName;

        Result<Submission> submissionResult = Submission.Create(
            command.FormId,
            command.DeviceId,
            command.UserId,
            dateTimeProvider.UtcNow,
            command.DisciplineId,
            command.TeacherId,
            command.DepartmentId,
            command.SpecialityId,
            command.SpecializationId,
            organizationName);

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
            Question? question = form.Questions.FirstOrDefault(q => q.Id == answerRequest.QuestionId);
            
            if (question is null)
            {
                return Result.Failure<Guid>(QuestionErrors.NotFound(answerRequest.QuestionId));
            }

            Result validationResult = ValidateAnswerForQuestionType(
                question.Type,
                answerRequest.Value,
                answerRequest.NumericValue,
                answerRequest.Weight);

            if (validationResult.IsFailure)
            {
                return Result.Failure<Guid>(validationResult.Error);
            }

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

    private static Result ValidateAnswerForQuestionType(
        QuestionType questionType,
        string? value,
        decimal? numericValue,
        decimal? weight)
    {
        return questionType switch
        {
            QuestionType.Text => value is not null && numericValue is null && weight is null
                ? Result.Success()
                : Result.Failure(AnswerErrors.InvalidTypeForText),

            QuestionType.Number => numericValue is not null && value is null && weight is null
                ? Result.Success()
                : Result.Failure(AnswerErrors.InvalidTypeForNumber),

            QuestionType.WeightedRating => numericValue is not null && weight is not null && value is null
                ? Result.Success()
                : Result.Failure(AnswerErrors.InvalidTypeForWeightedRating),

            _ => Result.Failure(AnswerErrors.UnknownQuestionType)
        };
    }
}
