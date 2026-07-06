using Application.Abstractions.Messaging;

namespace Application.Departments.Delete;

public sealed record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;
