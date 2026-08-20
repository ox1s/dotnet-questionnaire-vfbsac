using Domain.College.Departments;
using Domain.College.Disciplines;
using Domain.College.Specialities;
using Domain.College.Specializations;
using Domain.College.Teachers;
using Domain.Questionnaires.Forms;
using Domain.Questionnaires.Submissions;
using Domain.User;
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
    DbSet<TeacherDepartment> TeacherDepartments { get; }
    DbSet<Question> Questions { get; }
    DbSet<Speciality> Specialities { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<Answer> Answers { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
