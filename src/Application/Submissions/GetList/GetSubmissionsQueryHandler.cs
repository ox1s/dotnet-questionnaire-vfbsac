using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Submissions.GetList;

internal sealed class GetSubmissionsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetSubmissionsQuery, List<GetSubmissionsQueryResponse>>
{
    public async Task<Result<List<GetSubmissionsQueryResponse>>> Handle(
        GetSubmissionsQuery query, 
        CancellationToken cancellationToken)
    {
        IQueryable<Submission> submissionsQuery = context.Submissions.AsQueryable();

        if (query.FormId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.FormId == query.FormId);
        }

        if (!string.IsNullOrWhiteSpace(query.DeviceId))
        {
            submissionsQuery = submissionsQuery.Where(s => s.DeviceId == query.DeviceId);
        }

        if (query.UserId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.UserId == query.UserId);
        }

        if (query.DisciplineId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.DisciplineId == query.DisciplineId);
        }

        if (query.TeacherId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.TeacherId == query.TeacherId);
        }

        if (query.DepartmentId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.DepartmentId == query.DepartmentId);
        }

        if (query.SpecialityId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.SpecialityId == query.SpecialityId);
        }

        if (query.SpecializationId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.SpecializationId == query.SpecializationId);
        }

        if (!string.IsNullOrWhiteSpace(query.OrganizationName))
        {
            submissionsQuery = submissionsQuery.Where(s => s.Context.OrganizationName != null &&
                s.Context.OrganizationName.Contains(query.OrganizationName));
        }

        if (query.SubmittedFrom.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.SubmittedAt >= query.SubmittedFrom.Value);
        }

        if (query.SubmittedTo.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.SubmittedAt <= query.SubmittedTo.Value);
        }

        List<GetSubmissionsQueryResponse> submissions = await submissionsQuery
            .Include(s => s.Answers)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new GetSubmissionsQueryResponse
            {
                Id = s.Id,
                FormId = s.FormId,
                UserId = s.UserId,
                SubmittedAt = s.SubmittedAt,
                Context = new SubmissionContextResponse
                {
                    DisciplineId = s.Context.DisciplineId,
                    TeacherId = s.Context.TeacherId,
                    DepartmentId = s.Context.DepartmentId,
                    SpecialityId = s.Context.SpecialityId,
                    SpecializationId = s.Context.SpecializationId,
                    OrganizationName = s.Context.OrganizationName
                },
                Answers = s.Answers
                    .Select(a => new AnswerResponse
                    {
                        Id = a.Id,
                        QuestionId = a.QuestionId,
                        Value = a.Value,
                        NumericValue = a.NumericValue,
                        Weight = a.Weight
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return submissions;
    }
}
