using ErrorOr;
using MediatR;

namespace Questionnaire.Application.Forms.Commands.Delete;

public record DeleteFormCommand(int Id) : IRequest<ErrorOr<Success>>;