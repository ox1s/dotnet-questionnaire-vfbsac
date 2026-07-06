using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.College.Specialization;

internal sealed class SpecializationConfiguration : IEntityTypeConfiguration<Domain.College.Specializations.Specialization>
{
    public void Configure(EntityTypeBuilder<Domain.College.Specializations.Specialization> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(255);

        builder.Property(s => s.SpecialityId);
    }
}
