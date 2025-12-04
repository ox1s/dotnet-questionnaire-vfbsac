using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Contracts.Forms;

namespace Questionnaire.Application.Surveys.Queries.GetAvailable;

public sealed record GetAvailableSurveysQuery : IQuery<IEnumerable<FormResponse>>;