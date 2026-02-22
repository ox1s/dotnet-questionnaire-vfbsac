using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetComparative;

internal sealed class GetComparativeReportQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetComparativeReportQuery, List<ComparativeReportResponse>>
{
    public async Task<Result<List<ComparativeReportResponse>>> Handle(GetComparativeReportQuery query, CancellationToken cancellationToken)
    {
        Form? form = await context.Forms
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f => f.Id == query.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure<List<ComparativeReportResponse>>(Error.NotFound("Form.NotFound", "Form not found"));
        }

        var questions = form.Questions
            .Where(q => q.Type == QuestionType.Number || q.Type == QuestionType.Rating)
            .OrderBy(q => q.Order)
            .ToList();

        Dictionary<Guid, decimal?> statsA = await context.Submissions
            .Where(s => s.FormId == query.FormId && s.SubmittedAt >= query.PeriodA_Start && s.SubmittedAt <= query.PeriodA_End)
            .SelectMany(s => s.Answers)
            .GroupBy(a => a.QuestionId)
            .Select(g => new { QuestionId = g.Key, Avg = g.Average(x => x.NumericValue) })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Avg, cancellationToken);

        Dictionary<Guid, decimal?> statsB = await context.Submissions
            .Where(s => s.FormId == query.FormId && s.SubmittedAt >= query.PeriodB_Start && s.SubmittedAt <= query.PeriodB_End)
            .SelectMany(s => s.Answers)
            .GroupBy(a => a.QuestionId)
            .Select(g => new { QuestionId = g.Key, Avg = g.Average(x => x.NumericValue) })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Avg, cancellationToken);

        List<ComparativeReportResponse> result = [];

        foreach (Question q in questions)
        {
            decimal valA = statsA.TryGetValue(q.Id, out decimal? tempA) ? tempA ?? 0 : 0;
            decimal valB = statsB.TryGetValue(q.Id, out decimal? tempB) ? tempB ?? 0 : 0;

            result.Add(new ComparativeReportResponse(
                q.Text,
                (double)valA,
                (double)valB,
                (double)(valB - valA)
            ));
        }

        return result;
    }
}
