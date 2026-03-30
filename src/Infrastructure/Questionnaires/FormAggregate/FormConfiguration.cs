using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Domain.Questionnaires.Forms;

namespace Infrastructure.Questionnaires.FormAggregate;

internal sealed class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.IsActive)
            .IsRequired();

        var comparer = new ValueComparer<List<FilterField>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(f => f.RequiredFilters)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<List<FilterField>>(v, (JsonSerializerOptions?)null))
            .Metadata.SetValueComparer(comparer);

        builder.HasMany(f => f.Questions)
            .WithOne()
            .HasForeignKey(q => q.FormId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
