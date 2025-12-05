using Questionnaire.SharedKernel;

namespace Questionnaire.Domain.Users;

public class User : Entity
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
