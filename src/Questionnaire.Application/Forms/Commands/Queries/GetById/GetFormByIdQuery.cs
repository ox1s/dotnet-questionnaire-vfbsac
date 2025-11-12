using ErrorOr;
using MediatR;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Queries.GetById;

public record GetFormByIdQuery(int Id) : IRequest<ErrorOr<Form>>;