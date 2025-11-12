using ErrorOr;
using MediatR;
using Questionnaire.Application.Common.Interfaces;

namespace Questionnaire.Application.Forms.Commands.Delete;

public class DeleteFormCommandHandler : IRequestHandler<DeleteFormCommand, ErrorOr<Success>>
{
    private readonly IApplicationDbContext _context;

    public DeleteFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms.FindAsync(request.Id);
        if (form is null)
        {
            return Error.NotFound("Form not found.");
        }

        _context.Forms.Remove(form);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}