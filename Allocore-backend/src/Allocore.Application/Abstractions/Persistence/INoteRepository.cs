namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Notes;

public interface INoteRepository : IReadRepository<Note>, IWriteRepository<Note>
{
    Task<(IEnumerable<Note> Notes, int TotalCount)> GetPagedByEntityAsync(
        NoteEntityType entityType,
        Guid entityId,
        int page,
        int pageSize,
        NoteCategory? categoryFilter = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetPinnedByEntityAsync(
        NoteEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetRemindersForCompanyAsync(
        Guid companyId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
    Task<int> GetCountByEntityAsync(
        NoteEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
}
