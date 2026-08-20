using Application.Abstractions.Messaging;
using Domain.Questionnaires.Forms;
using Domain.User;

namespace Application.Forms.Create;

public sealed record CreateFormCommand(
    string Title,
    List<FilterField>? RequiredFilters = null,
    List<QuestionRequest>? Questions = null,
    UserRole? TargetRole = null)
    : ICommand<Guid>;
