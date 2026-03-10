namespace Allocore.Application.Features.Notes;

using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Entities.Notes;

internal static class NoteMapper
{
    public static NoteDto ToDto(Note note, string authorName) => new(
        note.Id,
        note.EntityType.ToString(),
        note.EntityId,
        note.AuthorUserId,
        authorName,
        note.Content,
        note.Category.ToString(),
        note.IsPinned,
        note.ReminderDate,
        note.CreatedAt,
        note.UpdatedAt);
}
