namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        // LocaleTag value object - use HasConversion for simpler mapping
        builder.Property(u => u.Locale)
            .HasConversion(
                l => l.Value,
                v => LocaleTag.Create(v))
            .HasColumnName("Locale")
            .HasMaxLength(10);
        
        // Password reset token (stored as hash for security)
        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(256);
        
        builder.HasIndex(u => u.PasswordResetToken);
    }
}
