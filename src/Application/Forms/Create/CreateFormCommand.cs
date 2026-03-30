using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;

namespace Application.Forms.Create;

public sealed record CreateFormCommand(
    string Title,
    List<FilterField>? RequiredFilters = null,
    List<QuestionRequest>? Questions = null)
    : ICommand<Guid>;
