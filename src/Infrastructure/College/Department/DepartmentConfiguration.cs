using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.Department;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Domain.College.Departments.Department>
{
    public void Configure(EntityTypeBuilder<Domain.College.Departments.Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => d.Name)
            .IsUnique();
    }
}
