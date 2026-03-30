using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.Teacher;

internal sealed class TeacherConfiguration : IEntityTypeConfiguration<Domain.College.Teachers.Teacher>
{
    public void Configure(EntityTypeBuilder<Domain.College.Teachers.Teacher> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.DepartmentId)
            .IsRequired();
    }
}
