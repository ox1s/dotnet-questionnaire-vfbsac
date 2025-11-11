using ErrorOr;
using MediatR;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Questions.Queries.GetAll;

public record GetAllQuestionsQuery : IRequest<ErrorOr<IEnumerable<Question>>>;