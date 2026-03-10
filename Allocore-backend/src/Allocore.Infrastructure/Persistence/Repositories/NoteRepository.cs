namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Notes;

public class NoteRepository : INoteRepository
{
    private readonly ApplicationDbContext _context;

    public NoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Notes.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<(IEnumerable<Note> Notes, int TotalCount)> GetPagedByEntityAsync(
        NoteEntityType entityType,
        Guid entityId,
        int page,
        int pageSize,
        NoteCategory? categoryFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notes
            .Where(n => n.EntityType == entityType && n.EntityId == entityId);

        if (categoryFilter.HasValue)
            query = query.Where(n => n.Category == categoryFilter.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (notes, totalCount);
    }

    public async Task<IEnumerable<Note>> GetPinnedByEntityAsync(
        NoteEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
        => await _context.Notes
            .Where(n => n.EntityType == entityType && n.EntityId == entityId && n.IsPinned)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Note>> GetRemindersForCompanyAsync(
        Guid companyId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
        => await _context.Notes
            .Where(n => n.CompanyId == companyId
                && n.ReminderDate != null
                && n.ReminderDate >= fromDate
                && n.ReminderDate <= toDate)
            .OrderBy(n => n.ReminderDate)
            .ToListAsync(cancellationToken);

    public async Task<int> GetCountByEntityAsync(
        NoteEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
        => await _context.Notes
            .CountAsync(n => n.EntityType == entityType && n.EntityId == entityId, cancellationToken);

    public async Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Notes.ToListAsync(cancellationToken);

    public async Task<Note> AddAsync(Note entity, CancellationToken cancellationToken = default)
    {
        await _context.Notes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Note entity, CancellationToken cancellationToken = default)
    {
        _context.Notes.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Note entity, CancellationToken cancellationToken = default)
    {
        _context.Notes.Remove(entity);
        return Task.CompletedTask;
    }
}
