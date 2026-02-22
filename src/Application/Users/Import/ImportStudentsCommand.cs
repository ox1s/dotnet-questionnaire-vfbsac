using Application.Abstractions.Messaging;
namespace Application.Users.Import;

public sealed record ImportStudentsCommand(Stream FileStream) : ICommand<int>;
