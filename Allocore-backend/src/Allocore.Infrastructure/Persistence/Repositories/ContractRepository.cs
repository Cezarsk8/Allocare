namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Contracts;

public class ContractRepository : IContractRepository
{
    private readonly ApplicationDbContext _context;

    public ContractRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Contracts
            .Include(c => c.Provider)
            .Include(c => c.ContractServices)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistsByContractNumberInCompanyAsync(Guid companyId, string contractNumber, CancellationToken cancellationToken = default)
        => await _context.Contracts.AnyAsync(
            c => c.CompanyId == companyId && c.ContractNumber == contractNumber,
            cancellationToken);

    public async Task<bool> ExistsByContractNumberInCompanyExcludingAsync(Guid companyId, string contractNumber, Guid excludeContractId, CancellationToken cancellationToken = default)
        => await _context.Contracts.AnyAsync(
            c => c.CompanyId == companyId && c.ContractNumber == contractNumber && c.Id != excludeContractId,
            cancellationToken);

    public async Task<(IEnumerable<Contract> Contracts, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        Guid? providerIdFilter = null,
        ContractStatus? statusFilter = null,
        bool? expiringWithinDays = null,
        int expiringDays = 30,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Include(c => c.Provider)
            .Include(c => c.ContractServices)
            .Where(c => c.CompanyId == companyId);

        if (providerIdFilter.HasValue)
            query = query.Where(c => c.ProviderId == providerIdFilter.Value);

        if (statusFilter.HasValue)
            query = query.Where(c => c.Status == statusFilter.Value);

        if (expiringWithinDays == true)
        {
            var cutoff = DateTime.UtcNow.AddDays(expiringDays);
            query = query.Where(c => c.EndDate != null && c.EndDate <= cutoff && c.EndDate >= DateTime.UtcNow);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(c =>
                c.Title.ToLower().Contains(term) ||
                (c.ContractNumber != null && c.ContractNumber.ToLower().Contains(term)) ||
                (c.Provider != null && c.Provider.Name.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var contracts = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (contracts, totalCount);
    }

    public async Task<IEnumerable<Contract>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await _context.Contracts
            .Include(c => c.ContractServices)
            .Where(c => c.ProviderId == providerId)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Contract>> GetExpiringContractsAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(withinDays);
        return await _context.Contracts
            .Include(c => c.Provider)
            .Where(c => c.CompanyId == companyId
                && c.EndDate != null
                && c.EndDate <= cutoff
                && c.EndDate >= DateTime.UtcNow
                && c.Status == ContractStatus.Active)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetContractsNeedingRenewalAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(withinDays);
        return await _context.Contracts
            .Include(c => c.Provider)
            .Where(c => c.CompanyId == companyId
                && c.RenewalDate != null
                && c.RenewalDate <= cutoff
                && c.RenewalDate >= DateTime.UtcNow
                && c.Status == ContractStatus.Active)
            .OrderBy(c => c.RenewalDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Contracts.ToListAsync(cancellationToken);

    public async Task<Contract> AddAsync(Contract entity, CancellationToken cancellationToken = default)
    {
        await _context.Contracts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Contract entity, CancellationToken cancellationToken = default)
    {
        _context.Contracts.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Contract entity, CancellationToken cancellationToken = default)
    {
        _context.Contracts.Remove(entity);
        return Task.CompletedTask;
    }
}
