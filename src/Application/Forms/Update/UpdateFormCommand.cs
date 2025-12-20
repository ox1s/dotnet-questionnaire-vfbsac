using Application.Abstractions.Messaging;
using Domain.Questionnaires.FormAggregate;

namespace Application.Forms.Update;

public sealed record UpdateFormCommand(
    Guid FormId,
    string Title,
    bool? IsActive = null,
    List<FilterField>? RequiredFilters = null)
    : ICommand;
