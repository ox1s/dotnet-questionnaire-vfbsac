namespace Questionnaire.Domain.Entities;

public class Form
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<FormQuestion> FormQuestions { get; set; } = new List<FormQuestion>();
    public ICollection<FormRole> FormRoles { get; set; } = new List<FormRole>();
}