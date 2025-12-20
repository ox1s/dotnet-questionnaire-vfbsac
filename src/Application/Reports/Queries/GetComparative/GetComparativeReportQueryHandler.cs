using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetComparative;

internal sealed class GetComparativeReportQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetComparativeReportQuery, List<ComparativeReportResponse>>
{
    // Исправлено имя параметра ct -> cancellationToken
    public async Task<Result<List<ComparativeReportResponse>>> Handle(GetComparativeReportQuery query, CancellationToken cancellationToken)
    {
        // Исправлено: явный тип Form?
        Form? form = await context.Forms
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f => f.Id == query.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure<List<ComparativeReportResponse>>(Error.NotFound("Form.NotFound", "Form not found"));
        }

        // Исправлено: явный тип List<Question>
        var questions = form.Questions
            .Where(q => q.Type == QuestionType.Number || q.Type == QuestionType.Rating)
            .OrderBy(q => q.Order)
            .ToList();

        // Исправлено: явный тип Dictionary<Guid, decimal?>
        Dictionary<Guid, decimal?> statsA = await context.Submissions
            .Where(s => s.FormId == query.FormId && s.SubmittedAt >= query.PeriodA_Start && s.SubmittedAt <= query.PeriodA_End)
            .SelectMany(s => s.Answers)
            .GroupBy(a => a.QuestionId)
            .Select(g => new { QuestionId = g.Key, Avg = g.Average(x => x.NumericValue) })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Avg, cancellationToken);

        // Исправлено: явный тип
        Dictionary<Guid, decimal?> statsB = await context.Submissions
            .Where(s => s.FormId == query.FormId && s.SubmittedAt >= query.PeriodB_Start && s.SubmittedAt <= query.PeriodB_End)
            .SelectMany(s => s.Answers)
            .GroupBy(a => a.QuestionId)
            .Select(g => new { QuestionId = g.Key, Avg = g.Average(x => x.NumericValue) })
            .ToDictionaryAsync(x => x.QuestionId, x => x.Avg, cancellationToken);

        // Исправлено: явный тип
        List<ComparativeReportResponse> result = [];

        foreach (Question q in questions)
        {
            // Исправлено: CA1854 (используем TryGetValue вместо ContainsKey + Indexer)
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
