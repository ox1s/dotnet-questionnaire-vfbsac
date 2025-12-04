using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Forms;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Surveys.Commands.Submit;

internal sealed class SubmitSurveyCommandHandler : ICommandHandler<SubmitSurveyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitSurveyCommandHandler(
        IApplicationDbContext context,
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(SubmitSurveyCommand command, CancellationToken cancellationToken)
    {
        int userId = _currentUserProvider.UserId;
        if (userId == 0)
        {
            return Result.Failure(Error.Validation("Auth.Unauthorized", "User is not authenticated."));
        }

        var form = await _context.Forms
            .Include(f => f.FormQuestions)
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure(FormErrors.NotFound(command.FormId));
        }

        var answer = new Answer
        {
            FormId = command.FormId,
            UserId = userId,
            SubmittedDate = _dateTimeProvider.UtcNow
        };

        var formQuestionIds = form.FormQuestions.Select(fq => fq.QuestionId).ToHashSet();

        foreach (var detailDto in command.Details)
        {
            if (!formQuestionIds.Contains(detailDto.QuestionId))
            {
                return Result.Failure(Error.Validation(
                    "Answer.InvalidQuestion",
                    $"Question with Id {detailDto.QuestionId} does not belong to this form."));
            }
            
            if (detailDto.Mark.HasValue && detailDto.Weight.HasValue && detailDto.Mark > detailDto.Weight)
            {
                return Result.Failure(Error.Validation(
                    "Answer.InvalidMark",
                    $"Mark for question {detailDto.QuestionId} cannot be greater than its weight."));
            }

            var answerDetail = new AnswerDetail
            {
                QuestionId = detailDto.QuestionId,
                Weight = detailDto.Weight,
                Mark = detailDto.Mark,
                TextResponse = detailDto.TextResponse
            };
            answer.Details.Add(answerDetail);
        }

        await _context.Answers.AddAsync(answer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}