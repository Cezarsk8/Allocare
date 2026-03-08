namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Users;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(rt => rt.TokenHash);
        
        builder.HasIndex(rt => rt.UserId);
        
        // Foreign key relationship to User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(rt => rt.DeviceInfo)
            .HasMaxLength(500);
        
        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(45); // IPv6 max length
    }
}
