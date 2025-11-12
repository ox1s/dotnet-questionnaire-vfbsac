using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Surveys.Queries.GetAvailable;

public class GetAvailableSurveysQueryHandler : IRequestHandler<GetAvailableSurveysQuery, ErrorOr<IEnumerable<Form>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetAvailableSurveysQueryHandler(IApplicationDbContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<IEnumerable<Form>>> Handle(GetAvailableSurveysQuery request, CancellationToken cancellationToken)
    {
        var userRoles = _currentUserProvider.Roles;

        var availableForms = await _context.Forms
            .Where(f => f.IsActive && f.FormRoles.Any(fr => userRoles.Contains(fr.Role.Name)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return availableForms;
    }
}