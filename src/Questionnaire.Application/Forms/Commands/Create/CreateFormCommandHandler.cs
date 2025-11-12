using ErrorOr;
using MediatR;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Commands.Create;

public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, ErrorOr<Form>>
{
    private readonly IApplicationDbContext _context;

    public CreateFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Form>> Handle(CreateFormCommand command, CancellationToken cancellationToken)
    {
        var form = new Form
        {
            Name = command.Name,
            IsActive = true 
        };

        await _context.Forms.AddAsync(form, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return form;
    }
}