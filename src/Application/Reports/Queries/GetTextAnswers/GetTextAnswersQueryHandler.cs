using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reports.Queries.Shared;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.Queries.GetTextAnswers;

internal sealed class GetTextAnswersQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTextAnswersQuery, List<GetTextAnswersQueryResponse>>
{
    public async Task<Result<List<GetTextAnswersQueryResponse>>> Handle(
        GetTextAnswersQuery query,
        CancellationToken cancellationToken)
    {
        bool formExists = await context.Forms
            .AnyAsync(f => f.Id == query.FormId, cancellationToken);

        if (!formExists)
        {
            return Result.Failure<List<GetTextAnswersQueryResponse>>(
                FormErrors.NotFound(query.FormId));
        }

        IQueryable<Guid> textQuestionIds = context.Questions
            .AsNoTracking()
            .Where(q => q.FormId == query.FormId && q.Type == QuestionType.Text)
            .Select(q => q.Id);

        IQueryable<Submission> submissionsQuery = context.Submissions
            .AsNoTracking()
            .Where(s => s.FormId == query.FormId);

        if (query.PeriodStart.HasValue && query.PeriodEnd.HasValue)
        {
            DateTime normalizedToDate = query.PeriodEnd.Value.AddDays(1);

            submissionsQuery = submissionsQuery
                .Where(s => s.SubmittedAt >= query.PeriodStart.Value && s.SubmittedAt < normalizedToDate);
        }

        IQueryable<Submission> filteredQuery = SubmissionFilterHelper.ApplyFilters(
            submissionsQuery,
            query.FilterSet);

        var rawAnswers = await filteredQuery
            .SelectMany(s => s.Answers, (submission, answer) => new { submission, answer })
            .Where(x =>
                textQuestionIds.Contains(x.answer.QuestionId) &&
                x.answer.Value != null &&
                x.answer.Value != "")
            .Select(x => new
            {
                x.answer.QuestionId,
                x.answer.Value,
                x.submission.SubmittedAt,
                x.submission.Context.TeacherId,
                x.submission.Context.DepartmentId
            })
            .ToListAsync(cancellationToken);

        if (rawAnswers.Count == 0)
        {
            return new List<GetTextAnswersQueryResponse>();
        }

        var questionIds = rawAnswers.Select(a => a.QuestionId).ToHashSet();

        Dictionary<Guid, string> questionTexts = await context.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Select(q => new { q.Id, q.Text })
            .ToDictionaryAsync(q => q.Id, q => q.Text, cancellationToken);

        IEnumerable<Guid> teacherIds = rawAnswers
            .Select(a => a.TeacherId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value);

        Dictionary<Guid, string> teacherNames = await EntityNameResolver.ResolveTeacherNamesAsync(
            teacherIds,
            context,
            cancellationToken);

        IEnumerable<Guid> departmentIds = rawAnswers
            .Select(a => a.DepartmentId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value);

        Dictionary<Guid, string> departmentNames = await EntityNameResolver.ResolveDepartmentNamesAsync(
            departmentIds,
            context,
            cancellationToken);

        var responses = rawAnswers
            .Select(a => new GetTextAnswersQueryResponse(
                a.QuestionId,
                questionTexts.GetValueOrDefault(a.QuestionId) ?? string.Empty,
                a.Value!,
                a.SubmittedAt,
                a.TeacherId,
                a.TeacherId.HasValue ? teacherNames.GetValueOrDefault(a.TeacherId.Value) : null,
                a.DepartmentId,
                a.DepartmentId.HasValue ? departmentNames.GetValueOrDefault(a.DepartmentId.Value) : null))
            .OrderByDescending(a => a.SubmittedAt)
            .ToList();

        return responses;
    }
}
