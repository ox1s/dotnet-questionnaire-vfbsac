using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Application.Forms.Common;
using Questionnaire.Domain.Forms;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Forms.Queries.GetAll;

internal sealed class GetAllFormsQueryHandler : IQueryHandler<GetAllFormsQuery, IEnumerable<FormResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllFormsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<FormResponse>>> Handle(GetAllFormsQuery query, CancellationToken cancellationToken)
    {
        var forms = await _context.Forms
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = forms.Select(f => new FormResponse(
            f.Id,
            f.Name,
            f.IsActive,
            null));

        return Result.Success(response);
    }
}
