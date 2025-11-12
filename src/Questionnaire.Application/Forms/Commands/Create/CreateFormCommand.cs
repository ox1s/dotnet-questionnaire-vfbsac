using ErrorOr;
using MediatR;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Commands.Create;

public record CreateFormCommand(string Name) : IRequest<ErrorOr<Form>>;