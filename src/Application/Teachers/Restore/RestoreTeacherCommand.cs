using Application.Abstractions.Messaging;

namespace Application.Teachers.Restore;

public sealed record RestoreTeacherCommand(Guid TeacherId) : ICommand;
