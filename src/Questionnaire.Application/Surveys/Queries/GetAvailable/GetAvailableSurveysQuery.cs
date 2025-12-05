using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Application.Forms.Common;

namespace Questionnaire.Application.Surveys.Queries.GetAvailable;

public sealed record GetAvailableSurveysQuery : IQuery<IEnumerable<FormResponse>>;