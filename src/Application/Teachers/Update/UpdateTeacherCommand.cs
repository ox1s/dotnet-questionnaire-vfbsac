using Application.Abstractions.Messaging;

namespace Application.Teachers.Update;

public sealed record UpdateTeacherCommand(
    Guid TeacherId,
    string FullName,
    IReadOnlyCollection<Guid>? DepartmentIds) : ICommand;
