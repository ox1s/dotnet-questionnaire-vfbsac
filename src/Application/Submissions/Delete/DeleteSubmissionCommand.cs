using Application.Abstractions.Messaging;

namespace Application.Submissions.Delete;

public sealed record DeleteSubmissionCommand(Guid SubmissionId) : ICommand;
