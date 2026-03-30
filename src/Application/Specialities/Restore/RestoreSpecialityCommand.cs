using Application.Abstractions.Messaging;

namespace Application.Specialities.Restore;

public sealed record RestoreSpecialityCommand(Guid SpecialityId) : ICommand;
