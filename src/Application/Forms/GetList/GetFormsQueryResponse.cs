using Domain.Questionnaires.Forms;
using Domain.User;

namespace Application.Forms.GetList;

public sealed record GetFormsQueryResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public List<FilterField>? RequiredFilters { get; init; }
    public UserRole? TargetRole { get; init; }
}
