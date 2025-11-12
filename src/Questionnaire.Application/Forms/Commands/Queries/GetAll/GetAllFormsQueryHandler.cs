using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Queries.GetAll;

public class GetAllFormsQueryHandler : IRequestHandler<GetAllFormsQuery, ErrorOr<IEnumerable<Form>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllFormsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IEnumerable<Form>>> Handle(GetAllFormsQuery request, CancellationToken cancellationToken)
    {
        var forms = await _context.Forms
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return forms;
    }
}