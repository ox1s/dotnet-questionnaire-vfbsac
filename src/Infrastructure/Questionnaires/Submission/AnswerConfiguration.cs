using Domain.Questionnaires.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Questionnaires.Submission;

internal sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.SubmissionId)
            .IsRequired();

        builder.Property(a => a.QuestionId)
            .IsRequired();

        builder.Property(a => a.Value)
            .HasMaxLength(5000);

        builder.Property(a => a.NumericValue)
            .HasPrecision(18, 2);

        builder.Property(a => a.Weight)
            .HasPrecision(18, 2);

        builder.HasIndex(a => new { a.SubmissionId, a.QuestionId })
            .IsUnique();
    }
}
