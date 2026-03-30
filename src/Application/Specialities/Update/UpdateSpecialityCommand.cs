using Application.Abstractions.Messaging;

namespace Application.Specialities.Update;

public sealed record UpdateSpecialityCommand(Guid SpecialityId, string Name) : ICommand;
