namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Providers;

public class ProviderRepository : IProviderRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Providers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Provider?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Providers
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameInCompanyAsync(Guid companyId, string name, CancellationToken cancellationToken = default)
        => await _context.Providers.AnyAsync(
            p => p.CompanyId == companyId && p.Name == name,
            cancellationToken);

    public async Task<bool> ExistsByNameInCompanyExcludingAsync(Guid companyId, string name, Guid excludeProviderId, CancellationToken cancellationToken = default)
        => await _context.Providers.AnyAsync(
            p => p.CompanyId == companyId && p.Name == name && p.Id != excludeProviderId,
            cancellationToken);

    public async Task<(IEnumerable<Provider> Providers, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        ProviderCategory? categoryFilter = null,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Providers
            .Include(p => p.Contacts)
            .Where(p => p.CompanyId == companyId);

        if (categoryFilter.HasValue)
            query = query.Where(p => p.Category == categoryFilter.Value);

        if (isActiveFilter.HasValue)
            query = query.Where(p => p.IsActive == isActiveFilter.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.LegalName != null && p.LegalName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var providers = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (providers, totalCount);
    }

    public async Task<IEnumerable<Provider>> GetAllByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
        => await _context.Providers
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Provider>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Providers.ToListAsync(cancellationToken);

    public async Task<Provider> AddAsync(Provider entity, CancellationToken cancellationToken = default)
    {
        await _context.Providers.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Provider entity, CancellationToken cancellationToken = default)
    {
        _context.Providers.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Provider entity, CancellationToken cancellationToken = default)
    {
        _context.Providers.Remove(entity);
        return Task.CompletedTask;
    }
}
