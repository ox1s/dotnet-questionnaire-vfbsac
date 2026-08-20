using Application.Abstractions.Messaging;

namespace Application.Forms.Deactivate;

public sealed record DeactivateFormCommand(Guid FormId) : ICommand;
