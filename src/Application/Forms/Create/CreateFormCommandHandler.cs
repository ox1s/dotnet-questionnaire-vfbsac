using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Forms.Create;

internal sealed class CreateFormCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateFormCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateFormCommand command, CancellationToken cancellationToken)
    {
        Result<Form> formResult = Form.Create(command.Title, command.RequiredFilters, command.TargetRole);
        if (formResult.IsFailure)
        {
            return Result.Failure<Guid>(formResult.Error);
        }

        Form form = formResult.Value;

        if (command.Questions is not null && command.Questions.Count > 0)
        {
            foreach (QuestionRequest questionRequest in command.Questions)
            {
                Result<Question> questionResult = form.AddQuestion(
                    questionRequest.Text,
                    questionRequest.Type,
                    questionRequest.Order);

                if (questionResult.IsFailure)
                {
                    return Result.Failure<Guid>(questionResult.Error);
                }
            }
        }

        context.Forms.Add(form);
        await context.SaveChangesAsync(cancellationToken);

        return form.Id;
    }
}
