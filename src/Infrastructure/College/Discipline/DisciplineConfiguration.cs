using Domain.College.Department;
using Domain.College.Discipline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.DisciplineAggregate;

internal sealed class DisciplineConfiguration : IEntityTypeConfiguration<Discipline>
{
    public void Configure(EntityTypeBuilder<Discipline> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.DepartmentId)
        .IsRequired();

        builder.HasIndex(d => d.Name)
            .IsUnique();

        builder.HasOne<Department>()
        .WithMany()
        .HasForeignKey(d => d.DepartmentId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
