namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Companies;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Name);

        builder.Property(c => c.LegalName)
            .HasMaxLength(300);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.HasIndex(c => c.TaxId)
            .IsUnique()
            .HasFilter("\"TaxId\" IS NOT NULL");

        builder.HasMany(c => c.UserCompanies)
            .WithOne(uc => uc.Company)
            .HasForeignKey(uc => uc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
