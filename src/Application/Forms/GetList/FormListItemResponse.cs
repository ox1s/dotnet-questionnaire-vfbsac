using Domain.Questionnaires.FormAggregate;

namespace Application.Forms.GetList;

public sealed record FormListItemResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public List<FilterField>? RequiredFilters { get; init; }
}
