namespace Questionnaire.Domain.Entities;

public class FormRole
{
    public int FormId { get; set; }
    public Form Form { get; set; } = null!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}