using Questionnaire.Domain.Forms;
using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Users;

public class Role : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "student", "admin", "departmentManager", etc.
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<FormRole> FormRoles { get; set; } = new List<FormRole>();
}
