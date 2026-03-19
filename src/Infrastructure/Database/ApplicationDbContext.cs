using Application.Abstractions.Data;
using Domain.College.DepartmentAggregate;
using Domain.College.DisciplineAggregate;
using Domain.College.SpecialityAggregate;
using Domain.College.SpecializationAggregate;
using Domain.College.TeacherAggregate;
using Domain.Questionnaires.FormAggregate;
using Domain.Questionnaires.SubmissionAggregate;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Form> Forms { get; set; }

    public DbSet<Submission> Submissions { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<Discipline> Disciplines { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Speciality> Specialities { get; set; }

    public DbSet<Specialization> Specializations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);


        modelBuilder.Entity<Discipline>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Speciality>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Specialization>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Form>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<Submission>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Teacher>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}
