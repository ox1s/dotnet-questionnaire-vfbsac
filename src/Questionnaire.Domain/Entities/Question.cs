using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Entities;

public class Question : Entity
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}