using Application.Abstractions.Messaging;

namespace Application.Forms.Activate;

public sealed record ActivateFormCommand(Guid FormId) : ICommand;
