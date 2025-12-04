using Questionnaire.Application.Abstractions.Messaging;

namespace Questionnaire.Application.Forms.Commands.Delete;

public sealed record DeleteFormCommand(int Id) : ICommand;