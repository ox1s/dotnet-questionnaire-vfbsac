using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using DomainQuestionType = Questionnaire.Domain.Entities.QuestionType;

namespace Questionnaire.Application.Reports.Queries.GetSummary;

public class GetSummaryReportQueryHandler : IRequestHandler<GetSummaryReportQuery, ErrorOr<SummaryReportResult>>
{
    private readonly IApplicationDbContext _context;

    public GetSummaryReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<SummaryReportResult>> Handle(GetSummaryReportQuery request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.FormQuestions)
                .ThenInclude(fq => fq.Question)
                .ThenInclude(q => q.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form is null)
        {
            return Error.NotFound("Form not found.");
        }

        var answers = await _context.Answers
            .Include(a => a.Details)
                .ThenInclude(d => d.SelectedOptions)
            .Where(a => a.FormId == request.FormId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allDetails = answers.SelectMany(a => a.Details).ToList();
        var questionSummaries = new List<QuestionSummaryResult>();

        foreach (var formQuestion in form.FormQuestions.OrderBy(fq => fq.Order))
        {
            var question = formQuestion.Question;
            var detailsForQuestion = allDetails.Where(d => d.QuestionId == question.Id).ToList();

            double avgMark = 0;
            double avgWeight = 0;
            int ratingCount = 0;
            var textResponses = new List<string>();
            var choiceCounts = new Dictionary<int, int>();

            switch (question.Type)
            {
                case DomainQuestionType.Rating:
                    var validDetails = detailsForQuestion.Where(d => d.Mark.HasValue && d.Weight.HasValue).ToList();
                    ratingCount = validDetails.Count;
                    if (ratingCount > 0)
                    {
                        avgMark = validDetails.Average(d => d.Mark!.Value);
                        avgWeight = validDetails.Average(d => d.Weight!.Value);
                    }
                    break;
                case DomainQuestionType.Text:
                    textResponses = detailsForQuestion
                        .Where(d => !string.IsNullOrEmpty(d.TextResponse))
                        .Select(d => d.TextResponse!)
                        .ToList();
                    break;
                case DomainQuestionType.Choice:
                    var selectedOptionIds = detailsForQuestion
                        .SelectMany(d => d.SelectedOptions)
                        .Select(so => so.QuestionOptionId);

                    choiceCounts = selectedOptionIds
                        .GroupBy(id => id)
                        .ToDictionary(g => g.Key, g => g.Count());
                    break;
            }

            questionSummaries.Add(new QuestionSummaryResult(
                question.Id,
                question.Text,
                question.Type,
                question.Options,
                avgMark,
                avgWeight,
                ratingCount,
                textResponses,
                choiceCounts));
        }

        return new SummaryReportResult(
            form.Id,
            form.Name,
            answers.Count,
            questionSummaries);
    }
}