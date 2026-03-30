using Application.Abstractions.Messaging;

namespace Application.Disciplines.Restore;

public sealed record RestoreDisciplineCommand(Guid DisciplineId) : ICommand;
