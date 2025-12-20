using Domain.Questionnaires.FormAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Questionnaires.FormAggregate;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.FormId)
            .IsRequired();

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(q => q.Order)
            .IsRequired();

        builder.HasIndex(q => new { q.FormId, q.Order })
            .IsUnique();
    }
}
