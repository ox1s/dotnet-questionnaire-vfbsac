using Application.Abstractions.Messaging;

namespace Application.Specialities.Delete;

public sealed record DeleteSpecialityCommand(Guid SpecialityId) : ICommand;
