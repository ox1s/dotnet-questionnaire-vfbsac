using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Contracts.Reports;
using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Forms;
using Questionnaire.Domain.Entities;
using Questionnaire.SharedKernel;
using Microsoft.EntityFrameworkCore;
using DomainQuestionType = Questionnaire.Domain.Entities.QuestionType;
using ContractsQuestionType = Questionnaire.Contracts.Questions.QuestionType;

namespace Questionnaire.Application.Reports.Queries.GetSummary;

internal sealed class GetSummaryReportQueryHandler : IQueryHandler<GetSummaryReportQuery, SummaryReportResponse>
{
    private readonly IApplicationDbContext _context;

    public GetSummaryReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SummaryReportResponse>> Handle(GetSummaryReportQuery query, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.FormQuestions)
                .ThenInclude(fq => fq.Question)
                .ThenInclude(q => q.Options)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FormId, cancellationToken);

        if (form is null)
        {
            return Result.Failure<SummaryReportResponse>(FormErrors.NotFound(query.FormId));
        }

        var answers = await _context.Answers
            .Include(a => a.Details)
                .ThenInclude(d => d.SelectedOptions)
            .Where(a => a.FormId == query.FormId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allDetails = answers.SelectMany(a => a.Details).ToList();
        var questionSummaries = new List<QuestionSummaryResponse>();

        foreach (var formQuestion in form.FormQuestions.OrderBy(fq => fq.Order))
        {
            var question = formQuestion.Question;
            var detailsForQuestion = allDetails.Where(d => d.QuestionId == question.Id).ToList();

            RatingSummaryData? ratingData = null;
            List<string>? textData = null;
            List<ChoiceSummaryData>? choiceData = null;

            switch (question.Type)
            {
                case DomainQuestionType.Rating:
                    var validDetails = detailsForQuestion.Where(d => d.Mark.HasValue && d.Weight.HasValue).ToList();
                    int ratingCount = validDetails.Count;
                    if (ratingCount > 0)
                    {
                        double avgMark = validDetails.Average(d => d.Mark!.Value);
                        double avgWeight = validDetails.Average(d => d.Weight!.Value);
                        ratingData = new RatingSummaryData(avgMark, avgWeight, ratingCount);
                    }
                    break;
                case DomainQuestionType.Text:
                    var responses = detailsForQuestion
                        .Where(d => !string.IsNullOrEmpty(d.TextResponse))
                        .Select(d => d.TextResponse!)
                        .ToList();
                    if (responses.Any())
                    {
                        textData = responses;
                    }
                    break;
                case DomainQuestionType.Choice:
                    var selectedOptionIds = detailsForQuestion
                        .SelectMany(d => d.SelectedOptions)
                        .Select(so => so.QuestionOptionId);

                    var choiceCounts = selectedOptionIds
                        .GroupBy(id => id)
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    if (choiceCounts.Any())
                    {
                        choiceData = question.Options
                            .Select(opt => new ChoiceSummaryData(
                                opt.Id,
                                opt.Text,
                                choiceCounts.TryGetValue(opt.Id, out int count) ? count : 0))
                            .ToList();
                    }
                    break;
            }

            var questionType = MapQuestionType(question.Type);
            questionSummaries.Add(new QuestionSummaryResponse(
                question.Id,
                question.Text,
                questionType,
                ratingData,
                textData,
                choiceData));
        }

        var response = new SummaryReportResponse(
            form.Id,
            form.Name,
            answers.Count,
            questionSummaries);

        return Result.Success(response);
    }

    private static ContractsQuestionType MapQuestionType(DomainQuestionType domainType)
    {
        return domainType switch
        {
            DomainQuestionType.Rating => ContractsQuestionType.Rating,
            DomainQuestionType.Text => ContractsQuestionType.Text,
            DomainQuestionType.Choice => ContractsQuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map domain question type to contract."),
        };
    }
}