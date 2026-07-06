using Application.Abstractions.Messaging;

namespace Application.Departments.Update;

public sealed record UpdateDepartmentCommand(Guid DepartmentId, string Name) : ICommand;
