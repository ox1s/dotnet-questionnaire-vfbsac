using Application.Abstractions.Messaging;

namespace Application.Forms.Delete;

public sealed record DeleteFormCommand(Guid FormId) : ICommand;
