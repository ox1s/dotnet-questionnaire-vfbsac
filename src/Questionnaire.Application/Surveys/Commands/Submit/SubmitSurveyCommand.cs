using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Surveys.Commands.Submit;

public record SubmitSurveyCommand(
    int FormId,
    List<AnswerDetailItem> Details) : IRequest<ErrorOr<Success>>;

