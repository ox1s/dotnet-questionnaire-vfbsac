using SharedKernel;

namespace Domain.College.Teachers;

public sealed class Teacher : Entity, ISoftDeletable
{
    private readonly List<TeacherDepartment> _departments = [];

    public string FullName { get; private set; }
    public bool IsDeleted { get; set; }

    public IReadOnlyCollection<Guid> DepartmentIds => _departments.Select(d => d.DepartmentId).ToList();

    private Teacher() { } // EF Core
    private Teacher(Guid id, string fullName) : base(id)
    {
        FullName = fullName;
    }

    public static Result<Teacher> Create(string fullName, IEnumerable<Guid>? departmentIds = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<Teacher>(Error.NullValue);
        }

        var teacher = new Teacher(Guid.NewGuid(), fullName.Trim());

        if (departmentIds is not null)
        {
            foreach (Guid departmentId in departmentIds.Distinct())
            {
                teacher.AssignDepartment(departmentId);
            }
        }

        return teacher;
    }

    public Result UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure(Error.NullValue);
        }

        FullName = fullName.Trim();
        return Result.Success();
    }

    public void AssignDepartment(Guid departmentId)
    {
        if (_departments.Any(d => d.DepartmentId == departmentId))
        {
            return;
        }

        _departments.Add(new TeacherDepartment(Id, departmentId));
    }

    public void RemoveDepartment(Guid departmentId)
    {
        _departments.RemoveAll(d => d.DepartmentId == departmentId);
    }
}
