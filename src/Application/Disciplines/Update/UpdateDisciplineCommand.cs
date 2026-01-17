using Application.Abstractions.Messaging;

namespace Application.Disciplines.Update;

public sealed record UpdateDisciplineCommand(
    Guid DisciplineId, 
    string Name, 
    Guid DepartmentId) : ICommand;
