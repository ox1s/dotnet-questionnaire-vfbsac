using Domain.Questionnaires.FormAggregate;

namespace Application.Forms.GetById;

public sealed record FormResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public List<FilterField>? RequiredFilters { get; init; }
    public List<QuestionResponse> Questions { get; init; } = [];
}

public sealed record QuestionResponse
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public QuestionType Type { get; init; }
    public int Order { get; init; }
}
