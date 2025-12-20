using Domain.College.DepartmentAggregate;
using Domain.College.DisciplineAggregate;
using Domain.College.SpecialityAggregate;
using Domain.College.SpecializationAggregate;
using Domain.College.TeacherAggregate;
using Domain.Questionnaires.FormAggregate;
using Domain.Questionnaires.SubmissionAggregate;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Form> Forms { get; }
    DbSet<Submission> Submissions { get; }
    DbSet<Department> Departments { get; }
    DbSet<Discipline> Disciplines { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Speciality> Specialities { get; }
    DbSet<Specialization> Specializations { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
