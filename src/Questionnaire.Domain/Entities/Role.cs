namespace Questionnaire.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "student", "admin", "departmentManager", etc.
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}