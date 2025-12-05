using Questionnaire.Contracts.Questions;
using Questionnaire.Domain.Questions;
using ContractsQuestionType = Questionnaire.Contracts.Questions.QuestionType;
using DomainQuestionType = Questionnaire.Domain.Questions.QuestionType;

namespace Questionnaire.Api.Common;

public static class ApiMappers
{
    public static QuestionResponse ToDto(Question question)
    {
        return new QuestionResponse(
            question.Id,
            question.Text,
            ToDto(question.Type),
            question.Options.Select(o => new OptionResponse(o.Id, o.Text)).ToList()
        );
    }

    private static ContractsQuestionType ToDto(DomainQuestionType domainType)
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