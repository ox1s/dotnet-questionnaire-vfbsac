using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.Teacher;

internal sealed class TeacherDepartmentConfiguration
    : IEntityTypeConfiguration<Domain.College.Teachers.TeacherDepartment>
{
    public void Configure(EntityTypeBuilder<Domain.College.Teachers.TeacherDepartment> builder)
    {
        builder.ToTable("teacher_departments");

        builder.HasKey(td => new { td.TeacherId, td.DepartmentId });

        builder.HasOne<Domain.College.Departments.Department>()
            .WithMany()
            .HasForeignKey(td => td.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
