using Application.Abstractions.Messaging;

namespace Application.Disciplines.Create;

public sealed record CreateDisciplineCommand(
    string Name,
    Guid DepartmentId) : ICommand<Guid>;
