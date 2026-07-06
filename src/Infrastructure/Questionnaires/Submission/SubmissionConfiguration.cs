using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Questionnaires.Submission;

internal sealed class SubmissionConfiguration : IEntityTypeConfiguration<Domain.Questionnaires.Submissions.Submission>
{
    public void Configure(EntityTypeBuilder<Domain.Questionnaires.Submissions.Submission> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FormId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.SubmittedAt)
            .IsRequired()
            .HasConversion(d => DateTime.SpecifyKind(d, DateTimeKind.Utc), v => v);

        builder.HasIndex(s => s.FormId);
        builder.HasIndex(s => new { s.FormId, s.SubmittedAt });

        builder.OwnsOne(s => s.Context, contextBuilder =>
        {
            contextBuilder.Property(c => c.TeacherId).HasColumnName("teacher_id");
            contextBuilder.Property(c => c.DisciplineId).HasColumnName("discipline_id");

            contextBuilder.Property(c => c.DepartmentId).HasColumnName("context_department_id");
            contextBuilder.Property(c => c.SpecialityId).HasColumnName("context_speciality_id");
            contextBuilder.Property(c => c.SpecializationId).HasColumnName("context_specialization_id");
            contextBuilder.Property(c => c.OrganizationName)
                .HasColumnName("context_organization_name")
                .HasMaxLength(255);

            contextBuilder.Property(c => c.EducationForm)
                .HasColumnName("context_education_form")
                .HasMaxLength(50); // ДФПО, ЗФПО

            contextBuilder.Property(c => c.EmployeeCategory)
                .HasColumnName("context_employee_category")
                .HasMaxLength(100); // АУП, ППС...

            contextBuilder.Property(c => c.Position)
                .HasColumnName("context_position")
                .HasMaxLength(255); // Должность

            contextBuilder.HasIndex(c => c.TeacherId);
            contextBuilder.HasIndex(c => c.DisciplineId);
            contextBuilder.HasIndex(c => c.DepartmentId);
            contextBuilder.HasIndex(c => c.SpecialityId);
            contextBuilder.HasIndex(c => c.SpecializationId);
        });

        builder.HasMany(s => s.Answers)
            .WithOne()
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
