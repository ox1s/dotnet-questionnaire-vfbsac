using Application.Abstractions.Messaging;

namespace Application.Teachers.Delete;

public sealed record DeleteTeacherCommand(Guid TeacherId) : ICommand;
