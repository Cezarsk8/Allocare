namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Providers;

public class ProviderContactConfiguration : IEntityTypeConfiguration<ProviderContact>
{
    public void Configure(EntityTypeBuilder<ProviderContact> builder)
    {
        builder.ToTable("ProviderContacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ProviderId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Email)
            .HasMaxLength(254);

        builder.Property(c => c.Phone)
            .HasMaxLength(30);

        builder.Property(c => c.Role)
            .HasMaxLength(100);

        builder.Property(c => c.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(c => c.ProviderId);

        // Relationship configured in ProviderConfiguration — no duplicate here
    }
}
