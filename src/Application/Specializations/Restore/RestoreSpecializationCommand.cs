using Application.Abstractions.Messaging;

namespace Application.Specializations.Restore;

public sealed record RestoreSpecializationCommand(Guid SpecializationId) : ICommand;
