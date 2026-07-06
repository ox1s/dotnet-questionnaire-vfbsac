using Application.Abstractions.Messaging;

namespace Application.Teachers.Create;

public sealed record CreateTeacherCommand(
    string FullName,
    Guid? DepartmentId) : ICommand<Guid>;
