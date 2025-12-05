using Questionnaire.Application.Authentication.Common;
using Questionnaire.Application.Forms.Common;
using Questionnaire.Application.Questions.Common;
using Questionnaire.Application.Reports.Common;
using Questionnaire.Contracts.Authentication;
using Questionnaire.Contracts.Forms;
using Questionnaire.Contracts.Questions;
using Questionnaire.Contracts.Reports;
using Questionnaire.Domain.Questions;

namespace Questionnaire.Api.Common;

public static class ApplicationToContractMappers
{
    public static Contracts.Authentication.AuthenticationResponse ToContract(
        Application.Authentication.Common.AuthenticationResponse response)
    {
        return new Contracts.Authentication.AuthenticationResponse(
            response.Id,
            response.Login,
            response.Token);
    }

    public static Contracts.Forms.FormResponse ToContract(Application.Forms.Common.FormResponse response)
    {
        return new Contracts.Forms.FormResponse(
            response.Id,
            response.Name,
            response.IsActive,
            response.Questions?.Select(ToContract).ToList());
    }

    public static Contracts.Questions.QuestionResponse ToContract(Application.Questions.Common.QuestionResponse response)
    {
        return new Contracts.Questions.QuestionResponse(
            response.Id,
            response.Text,
            ToContract(response.Type),
            response.Options.Select(ToContract).ToList());
    }

    public static Contracts.Questions.OptionResponse ToContract(Application.Questions.Common.OptionResponse response)
    {
        return new Contracts.Questions.OptionResponse(
            response.Id,
            response.Text);
    }

    public static Contracts.Reports.SummaryReportResponse ToContract(
        Application.Reports.Common.SummaryReportResponse response)
    {
        return new Contracts.Reports.SummaryReportResponse(
            response.FormId,
            response.FormName,
            response.TotalSubmissions,
            response.Questions.Select(ToContract).ToList());
    }

    public static Contracts.Reports.QuestionSummaryResponse ToContract(
        Application.Reports.Common.QuestionSummaryResponse response)
    {
        return new Contracts.Reports.QuestionSummaryResponse(
            response.QuestionId,
            response.QuestionText,
            ToContract(response.QuestionType),
            response.RatingData != null ? ToContract(response.RatingData) : null,
            response.TextData,
            response.ChoiceData?.Select(ToContract).ToList());
    }

    public static Contracts.Reports.RatingSummaryData ToContract(
        Application.Reports.Common.RatingSummaryData data)
    {
        return new Contracts.Reports.RatingSummaryData(
            data.AverageMark,
            data.AverageWeight,
            data.ResponseCount);
    }

    public static Contracts.Reports.ChoiceSummaryData ToContract(
        Application.Reports.Common.ChoiceSummaryData data)
    {
        return new Contracts.Reports.ChoiceSummaryData(
            data.OptionId,
            data.OptionText,
            data.SelectedCount);
    }

    public static Contracts.Questions.QuestionType ToContract(QuestionType domainType)
    {
        return domainType switch
        {
            QuestionType.Rating => Contracts.Questions.QuestionType.Rating,
            QuestionType.Text => Contracts.Questions.QuestionType.Text,
            QuestionType.Choice => Contracts.Questions.QuestionType.Choice,
            _ => throw new InvalidOperationException("Cannot map domain question type to contract."),
        };
    }
}
