namespace Domain.College.Teachers;

/// <summary>
/// Join entity linking a <see cref="Teacher"/> to a <c>Department</c> in a many-to-many relationship.
/// </summary>
public sealed class TeacherDepartment
{
    public Guid TeacherId { get; private set; }
    public Guid DepartmentId { get; private set; }

    private TeacherDepartment() { } // EF Core

    internal TeacherDepartment(Guid teacherId, Guid departmentId)
    {
        TeacherId = teacherId;
        DepartmentId = departmentId;
    }
}
