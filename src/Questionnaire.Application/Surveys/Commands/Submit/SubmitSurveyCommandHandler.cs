using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Surveys.Commands.Submit;

public class SubmitSurveyCommandHandler : IRequestHandler<SubmitSurveyCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SubmitSurveyCommandHandler(IApplicationDbContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<Success>> Handle(SubmitSurveyCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserProvider.UserId;
        if (userId == 0)
        {
            return Error.Unauthorized();
        }

        var form = await _context.Forms
            .Include(f => f.FormQuestions)
            .FirstOrDefaultAsync(f => f.Id == command.FormId, cancellationToken);

        if (form is null)
        {
            return Error.NotFound(description: "Form not found.");
        }

        var answer = new Answer
        {
            FormId = command.FormId,
            UserId = userId,
            SubmittedDate = DateTime.UtcNow
        };

        var formQuestionIds = form.FormQuestions.Select(fq => fq.QuestionId).ToHashSet();

        foreach (var detailDto in command.Details)
        {
            if (!formQuestionIds.Contains(detailDto.QuestionId))
            {
                return Error.Validation(description: $"Question with Id {detailDto.QuestionId} does not belong to this form.");
            }
            
            if (detailDto.Mark.HasValue && detailDto.Weight.HasValue && detailDto.Mark > detailDto.Weight)
            {
                return Error.Validation(
                    code: "Answer.InvalidMark",
                    description: $"Mark for question {detailDto.QuestionId} cannot be greater than its weight.");
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

        return Result.Success;
    }
}