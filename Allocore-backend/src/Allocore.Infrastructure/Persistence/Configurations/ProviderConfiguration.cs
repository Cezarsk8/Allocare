namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Providers;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CompanyId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.LegalName)
            .HasMaxLength(300);

        builder.Property(p => p.TaxId)
            .HasMaxLength(50);

        builder.Property(p => p.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Website)
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique provider name within a company
        builder.HasIndex(p => new { p.CompanyId, p.Name })
            .IsUnique();

        builder.HasIndex(p => p.CompanyId);

        builder.HasIndex(p => p.Category);

        builder.HasMany(p => p.Contacts)
            .WithOne(c => c.Provider)
            .HasForeignKey(c => c.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
