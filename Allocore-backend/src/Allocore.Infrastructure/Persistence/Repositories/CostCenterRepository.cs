namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.CostCenters;

public class CostCenterRepository : ICostCenterRepository
{
    private readonly ApplicationDbContext _context;

    public CostCenterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.CostCenters.FirstOrDefaultAsync(cc => cc.Id == id, cancellationToken);

    public async Task<IEnumerable<CostCenter>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.CostCenters.ToListAsync(cancellationToken);

    public async Task<bool> ExistsByCodeInCompanyAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
        => await _context.CostCenters.AnyAsync(
            cc => cc.CompanyId == companyId && cc.Code == code.Trim().ToUpper(),
            cancellationToken);

    public async Task<bool> ExistsByCodeInCompanyExcludingAsync(Guid companyId, string code, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.CostCenters.AnyAsync(
            cc => cc.CompanyId == companyId && cc.Code == code.Trim().ToUpper() && cc.Id != excludeId,
            cancellationToken);

    public async Task<(IEnumerable<CostCenter> CostCenters, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CostCenters.Where(cc => cc.CompanyId == companyId);

        if (isActiveFilter.HasValue)
            query = query.Where(cc => cc.IsActive == isActiveFilter.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(cc =>
                cc.Code.ToLower().Contains(term) ||
                cc.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var costCenters = await query
            .OrderBy(cc => cc.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (costCenters, totalCount);
    }

    public async Task<int> GetEmployeeCountAsync(Guid costCenterId, CancellationToken cancellationToken = default)
        => await _context.Employees.CountAsync(e => e.CostCenterId == costCenterId && e.IsActive, cancellationToken);

    public async Task<CostCenter> AddAsync(CostCenter entity, CancellationToken cancellationToken = default)
    {
        await _context.CostCenters.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(CostCenter entity, CancellationToken cancellationToken = default)
    {
        _context.CostCenters.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CostCenter entity, CancellationToken cancellationToken = default)
    {
        _context.CostCenters.Remove(entity);
        return Task.CompletedTask;
    }
}
