using Application.Abstractions.Messaging;

namespace Application.Teachers.Create;

public sealed record CreateTeacherCommand(
    string FullName,
    IReadOnlyCollection<Guid>? DepartmentIds) : ICommand<Guid>;
