using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Application.Forms.Common;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Questionnaire.Application.Surveys.Queries.GetAvailable;

internal sealed class GetAvailableSurveysQueryHandler : IQueryHandler<GetAvailableSurveysQuery, IEnumerable<FormResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetAvailableSurveysQueryHandler(IApplicationDbContext context, ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<Result<IEnumerable<FormResponse>>> Handle(GetAvailableSurveysQuery query, CancellationToken cancellationToken)
    {
        var userRoles = _currentUserProvider.Roles;

        var availableForms = await _context.Forms
            .Where(f => f.IsActive && f.FormRoles.Any(fr => userRoles.Contains(fr.Role.Name)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = availableForms.Select(f => new FormResponse(
            f.Id,
            f.Name,
            f.IsActive,
            null));

        return Result.Success(response);
    }
}