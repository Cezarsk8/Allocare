namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Contracts;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyId)
            .IsRequired();

        builder.Property(c => c.ProviderId)
            .IsRequired();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.ContractNumber)
            .HasMaxLength(100);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.BillingFrequency)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.TotalValue)
            .HasPrecision(18, 2);

        builder.Property(c => c.Currency)
            .HasMaxLength(3);

        builder.Property(c => c.PaymentTerms)
            .HasMaxLength(500);

        builder.Property(c => c.PriceConditions)
            .HasMaxLength(2000);

        builder.Property(c => c.LegalTeamContact)
            .HasMaxLength(300);

        builder.Property(c => c.InternalOwner)
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.TermsAndConditions)
            .HasColumnType("text");

        builder.HasIndex(c => c.CompanyId);
        builder.HasIndex(c => c.ProviderId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => new { c.CompanyId, c.ProviderId });
        builder.HasIndex(c => c.EndDate);
        builder.HasIndex(c => c.RenewalDate);

        builder.HasIndex(c => new { c.CompanyId, c.ContractNumber })
            .IsUnique()
            .HasFilter("\"ContractNumber\" IS NOT NULL");

        builder.HasOne(c => c.Provider)
            .WithMany()
            .HasForeignKey(c => c.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.ContractServices)
            .WithOne(cs => cs.Contract)
            .HasForeignKey(cs => cs.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(c => c.IsExpired);
    }
}
