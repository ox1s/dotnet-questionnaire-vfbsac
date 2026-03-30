using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.Speciality;

internal sealed class SpecialityConfiguration : IEntityTypeConfiguration<Domain.College.Specialities.Speciality>
{
    public void Configure(EntityTypeBuilder<Domain.College.Specialities.Speciality> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(255);
    }
}
