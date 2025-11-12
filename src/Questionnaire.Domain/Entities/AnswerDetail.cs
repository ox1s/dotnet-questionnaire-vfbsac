namespace Questionnaire.Domain.Entities;

public class AnswerDetail
{
    public int Id { get; set; }
    public int AnswerId { get; set; }
    public Answer Answer { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    // Для QuestionType.Rating
    public int? Weight { get; set; } 
    public int? Mark { get; set; }   

    // Для QuestionType.Text
    public string? TextResponse { get; set; }

    // Для QuestionType.Choice
    public ICollection<AnswerDetailSelectedOption> SelectedOptions { get; set; } = new List<AnswerDetailSelectedOption>();
}