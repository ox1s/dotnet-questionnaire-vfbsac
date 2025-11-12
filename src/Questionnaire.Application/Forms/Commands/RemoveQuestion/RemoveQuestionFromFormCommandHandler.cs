using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;

namespace Questionnaire.Application.Forms.Commands.RemoveQuestion;

public class RemoveQuestionFromFormCommandHandler : IRequestHandler<RemoveQuestionFromFormCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public RemoveQuestionFromFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(RemoveQuestionFromFormCommand request, CancellationToken cancellationToken)
    {
        var formQuestion = await _context.FormQuestions
            .FirstOrDefaultAsync(fq => fq.FormId == request.FormId && fq.QuestionId == request.QuestionId, cancellationToken);

        if (formQuestion is null)
        {
            return Error.NotFound("The specified question is not found in this form.");
        }

        _context.FormQuestions.Remove(formQuestion);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }



}