using Application.Abstractions.Messaging;

namespace Application.Specializations.Update;

public sealed record UpdateSpecializationCommand(
    Guid SpecializationId,
    string Name,
    Guid SpecialityId) : ICommand;
