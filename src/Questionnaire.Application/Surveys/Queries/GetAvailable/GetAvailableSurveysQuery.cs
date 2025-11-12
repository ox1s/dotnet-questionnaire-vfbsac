using ErrorOr;
using MediatR;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Surveys.Queries.GetAvailable;

public record GetAvailableSurveysQuery : IRequest<ErrorOr<IEnumerable<Form>>>;