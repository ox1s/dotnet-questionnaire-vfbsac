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

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);

        modelBuilder.Entity<Teacher>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Discipline>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Form>().HasQueryFilter(f => !f.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SaveDomainEventsToOutboxAsync();

        int result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task SaveDomainEventsToOutboxAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(aggregateRoot =>
            {
                List<IDomainEvent> domainEvents = aggregateRoot.DomainEvents;

                aggregateRoot.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            var outboxMessage = OutboxMessage.Create(domainEvent);
            OutboxMessages.Add(outboxMessage);
        }

        await Task.CompletedTask;
    }
}
