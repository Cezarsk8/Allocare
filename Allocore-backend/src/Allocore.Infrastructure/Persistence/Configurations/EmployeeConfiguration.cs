namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Employees;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CompanyId)
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.JobTitle)
            .HasMaxLength(200);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique email within a company
        builder.HasIndex(e => new { e.CompanyId, e.Email })
            .IsUnique();

        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.CostCenterId);

        // FK to CostCenter with SetNull on delete
        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
