using Questionnaire.Application.Abstractions.Messaging;
using Questionnaire.Domain.Forms;

namespace Questionnaire.Application.Forms.Commands.Create;

public sealed record CreateFormCommand(string Name) : ICommand<Form>;