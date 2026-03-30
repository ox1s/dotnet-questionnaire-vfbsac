using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.Discipline;

internal sealed class DisciplineConfiguration
    : IEntityTypeConfiguration<Domain.College.Disciplines.Discipline>
{
    public void Configure(EntityTypeBuilder<Domain.College.Disciplines.Discipline> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.DepartmentId)
            .IsRequired();

        builder.HasIndex(d => d.Name)
            .IsUnique();

        builder.HasOne<Domain.College.Departments.Department>()
            .WithMany()
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
