namespace Allocore.Domain.Entities.Notes;

using Allocore.Domain.Common;

public class Note : Entity
{
    public Guid CompanyId { get; private set; }
    public NoteEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public NoteCategory Category { get; private set; } = NoteCategory.General;
    public bool IsPinned { get; private set; }
    public DateTime? ReminderDate { get; private set; }

    private Note() { } // EF Core

    public static Note Create(
        Guid companyId,
        NoteEntityType entityType,
        Guid entityId,
        Guid authorUserId,
        string content,
        NoteCategory category = NoteCategory.General,
        bool isPinned = false,
        DateTime? reminderDate = null)
    {
        return new Note
        {
            CompanyId = companyId,
            EntityType = entityType,
            EntityId = entityId,
            AuthorUserId = authorUserId,
            Content = content,
            Category = category,
            IsPinned = isPinned,
            ReminderDate = reminderDate
        };
    }

    public void Update(string content, NoteCategory category, bool isPinned, DateTime? reminderDate)
    {
        Content = content;
        Category = category;
        IsPinned = isPinned;
        ReminderDate = reminderDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pin()
    {
        IsPinned = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpin()
    {
        IsPinned = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
