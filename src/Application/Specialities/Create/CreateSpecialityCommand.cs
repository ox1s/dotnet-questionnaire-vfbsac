using Application.Abstractions.Messaging;

namespace Application.Specialities.Create;

public sealed record CreateSpecialityCommand(string Name) : ICommand<Guid>;
