using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Forms;
using Questionnaire.SharedKernel;

namespace Questionnaire.Application.Forms.Commands.Create;

internal sealed class CreateFormCommandHandler : ICommandHandler<CreateFormCommand, Form>
{
    private readonly IApplicationDbContext _context;

    public CreateFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Form>> Handle(CreateFormCommand command, CancellationToken cancellationToken)
    {
        var form = new Form
        {
            Name = command.Name,
            IsActive = true 
        };

        await _context.Forms.AddAsync(form, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(form);
    }
}