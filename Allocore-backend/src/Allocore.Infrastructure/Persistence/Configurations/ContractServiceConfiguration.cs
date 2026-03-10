namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Contracts;

public class ContractServiceConfiguration : IEntityTypeConfiguration<ContractService>
{
    public void Configure(EntityTypeBuilder<ContractService> builder)
    {
        builder.ToTable("ContractServices");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.ContractId)
            .IsRequired();

        builder.Property(cs => cs.ServiceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cs => cs.ServiceDescription)
            .HasMaxLength(1000);

        builder.Property(cs => cs.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(cs => cs.UnitType)
            .HasMaxLength(50);

        builder.Property(cs => cs.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(cs => cs.ContractId);

        builder.HasOne(cs => cs.Contract)
            .WithMany(c => c.ContractServices)
            .HasForeignKey(cs => cs.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
