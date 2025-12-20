using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;

namespace Application.Forms.Create;

public sealed record CreateFormCommand(
    string Title,
    List<FilterField>? RequiredFilters = null,
    List<QuestionRequest>? Questions = null)
    : ICommand<Guid>;

public sealed record QuestionRequest(
    string Text,
    QuestionType Type,
    int Order);
