using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetAdvices;

internal sealed class GetAdvicesQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetAdvicesQuery, List<AdvicesQueryResponse>>
{
    public async Task<Result<List<AdvicesQueryResponse>>> Handle(
        GetAdvicesQuery query,
        CancellationToken cancellationToken)
    {
        bool formExists = await context.Forms
            .AnyAsync(f => f.Id == query.FormId, cancellationToken);

        if (!formExists)
        {
            return Result.Failure<List<AdvicesQueryResponse>>(
                FormErrors.NotFound(query.FormId));
        }

        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s => s.FormId == query.FormId);

        if (query.TeacherId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.TeacherId == query.TeacherId);
        }

        List<AdvicesQueryResponse> responses = await submissionsQuery
            .SelectMany(s => s.Answers, (submission, answer) => new { submission, answer })
            .Where(x => x.answer.Value != null && x.answer.Value != "")
            .Select(x => new AdvicesQueryResponse(
                x.answer.Value!,
                x.submission.Context.TeacherId,
                x.submission.Context.DepartmentId))
            .ToListAsync(cancellationToken);

        return responses;
    }
}
