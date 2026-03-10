namespace Allocore.Application.Features.Notes.DTOs;

public record CreateNoteRequest(
    string Content,
    string? Category,
    bool IsPinned,
    DateTime? ReminderDate
);
