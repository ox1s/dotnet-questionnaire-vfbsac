using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.Content)
            .IsRequired();

        builder.Property(o => o.OccurredOn)
            .IsRequired();

        builder.Property(o => o.ProcessedOn);

        builder.Property(o => o.Error)
            .HasMaxLength(1000);

        builder.ToTable("OutboxMessages", Schemas.Default);
    }
}
