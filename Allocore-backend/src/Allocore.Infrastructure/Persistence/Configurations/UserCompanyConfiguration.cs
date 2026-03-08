namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Companies;

public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.ToTable("UserCompanies");

        builder.HasKey(uc => uc.Id);

        builder.HasIndex(uc => new { uc.CompanyId, uc.UserId })
            .IsUnique();

        builder.HasIndex(uc => uc.UserId);
        builder.HasIndex(uc => uc.CompanyId);

        builder.Property(uc => uc.RoleInCompany)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserCompanies)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Company)
            .WithMany(c => c.UserCompanies)
            .HasForeignKey(uc => uc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
