namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.CostCenters;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");

        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.CompanyId)
            .IsRequired();

        builder.Property(cc => cc.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cc => cc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cc => cc.Description)
            .HasMaxLength(2000);

        builder.Property(cc => cc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique code within a company
        builder.HasIndex(cc => new { cc.CompanyId, cc.Code })
            .IsUnique();

        builder.HasIndex(cc => cc.CompanyId);
    }
}
