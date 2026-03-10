namespace Allocore.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allocore.Domain.Entities.Notes;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.CompanyId)
            .IsRequired();

        builder.Property(n => n.EntityType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.EntityId)
            .IsRequired();

        builder.Property(n => n.AuthorUserId)
            .IsRequired();

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(n => n.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.IsPinned)
            .IsRequired()
            .HasDefaultValue(false);

        // Composite index for querying notes by entity
        builder.HasIndex(n => new { n.EntityType, n.EntityId });

        builder.HasIndex(n => n.CompanyId);
        builder.HasIndex(n => n.AuthorUserId);
        builder.HasIndex(n => n.Category);
        builder.HasIndex(n => n.ReminderDate)
            .HasFilter("\"ReminderDate\" IS NOT NULL");

        // No FK on EntityId — polymorphic association
        // No FK on AuthorUserId — avoid cascade complexity; enforce at app layer
    }
}
