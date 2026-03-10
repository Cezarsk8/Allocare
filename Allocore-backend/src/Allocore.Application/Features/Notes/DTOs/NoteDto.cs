namespace Allocore.Application.Features.Notes.DTOs;

public record NoteDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    Guid AuthorUserId,
    string AuthorName,
    string Content,
    string Category,
    bool IsPinned,
    DateTime? ReminderDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
