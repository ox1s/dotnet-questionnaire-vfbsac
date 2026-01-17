using Application.Abstractions.Messaging;

namespace Application.Disciplines.Delete;

public sealed record DeleteDisciplineCommand(Guid DisciplineId) : ICommand;
