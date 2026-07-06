using Application.Abstractions.Messaging;

namespace Application.Departments.Restore;

public sealed record RestoreDepartmentCommand(Guid DepartmentId) : ICommand;
