using ErrorOr;
using MediatR;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Queries.GetAll;

public record GetAllFormsQuery : IRequest<ErrorOr<IEnumerable<Form>>>;