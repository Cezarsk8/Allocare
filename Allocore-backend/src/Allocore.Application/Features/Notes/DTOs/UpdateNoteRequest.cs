namespace Allocore.Application.Features.Notes.DTOs;

public record UpdateNoteRequest(
    string Content,
    string? Category,
    bool IsPinned,
    DateTime? ReminderDate
);
