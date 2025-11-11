namespace Questionnaire.Domain.Entities;

public class Answer
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public Form Form { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    public int? DisciplineId { get; set; }
    public int? TeacherId { get; set; }

    public ICollection<AnswerDetail> Details { get; set; } = new List<AnswerDetail>();
}