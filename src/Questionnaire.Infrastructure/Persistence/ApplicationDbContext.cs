using Microsoft.EntityFrameworkCore;
using Questionnaire.Application.Common.Interfaces;
using Questionnaire.Domain.Entities;

namespace Questionnaire.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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

        base.OnModelCreating(builder);
    }
}