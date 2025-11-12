namespace Questionnaire.Domain.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public ICollection<AnswerDetailSelectedOption> AnswerDetailSelectedOptions { get; set; } = new List<AnswerDetailSelectedOption>();
}
