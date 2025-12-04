using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Application.Forms.Commands.Create;

public sealed record CreateFormCommand(string Name) : ICommand<Form>;