using Domain.Questionnaires.SubmissionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Questionnaires.SubmissionAggregate;

internal sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FormId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.SubmittedAt)
            .IsRequired()
            .HasConversion(d => DateTime.SpecifyKind(d, DateTimeKind.Utc), v => v);

        builder.OwnsOne(s => s.Context, contextBuilder =>
                {
                    contextBuilder.Property(c => c.TeacherId).HasColumnName("teacher_id");
                    contextBuilder.Property(c => c.DisciplineId).HasColumnName("discipline_id");
                    // contextBuilder.Property(c => c.DepartmentId).HasColumnName("department_id");
                    // contextBuilder.Property(c => c.SpecialityId).HasColumnName("speciality_id");
                    // contextBuilder.Property(c => c.SpecializationId).HasColumnName("specialization_id");
                    // contextBuilder.Property(c => c.OrganizationName).HasColumnName("organization_name");
                });

        builder.HasMany(s => s.Answers)
            .WithOne()
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.FormId, s.UserId });
    }
}
