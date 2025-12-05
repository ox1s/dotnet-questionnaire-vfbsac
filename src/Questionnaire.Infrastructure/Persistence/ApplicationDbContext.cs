using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Abstractions;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Answers;
using Questionnaire.Domain.Forms;
using Questionnaire.Domain.Questions;
using Questionnaire.Domain.Users;
using Questionnaire.SharedKernel;
using static Questionnaire.Infrastructure.Persistence.Schemas;

namespace Questionnaire.Infrastructure.Persistence;

internal sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<FormQuestion> FormQuestions => Set<FormQuestion>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<AnswerDetail> AnswerDetails => Set<AnswerDetail>();
    public DbSet<AnswerDetailSelectedOption> AnswerDetailSelectedOptions => Set<AnswerDetailSelectedOption>();
    public DbSet<FormRole> FormRoles => Set<FormRole>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        await PublishDomainEventsAsync();
        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        List<IDomainEvent> domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
        builder.Entity<FormQuestion>()
            .HasKey(fq => new { fq.FormId, fq.QuestionId });
        builder.Entity<FormRole>()
            .HasKey(fr => new { fr.FormId, fr.RoleId });
        builder.Entity<AnswerDetailSelectedOption>()
            .HasKey(aso => new { aso.AnswerDetailId, aso.QuestionOptionId });
            
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.HasDefaultSchema(Schemas.Default);

        base.OnModelCreating(builder);
    }
}