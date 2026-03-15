namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Employees;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Employee?> GetByIdWithCostCenterAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Employees
            .Include(e => e.CostCenter)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Employees.ToListAsync(cancellationToken);

    public async Task<bool> ExistsByEmailInCompanyAsync(Guid companyId, string email, CancellationToken cancellationToken = default)
        => await _context.Employees.AnyAsync(
            e => e.CompanyId == companyId && e.Email == email.Trim().ToLower(),
            cancellationToken);

    public async Task<bool> ExistsByEmailInCompanyExcludingAsync(Guid companyId, string email, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Employees.AnyAsync(
            e => e.CompanyId == companyId && e.Email == email.Trim().ToLower() && e.Id != excludeId,
            cancellationToken);

    public async Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        Guid? costCenterIdFilter = null,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.CostCenter)
            .Where(e => e.CompanyId == companyId);

        if (costCenterIdFilter.HasValue)
            query = query.Where(e => e.CostCenterId == costCenterIdFilter.Value);

        if (isActiveFilter.HasValue)
            query = query.Where(e => e.IsActive == isActiveFilter.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var employees = await query
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (employees, totalCount);
    }

    public async Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedByCostCenterAsync(
        Guid costCenterId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .Include(e => e.CostCenter)
            .Where(e => e.CostCenterId == costCenterId);

        var totalCount = await query.CountAsync(cancellationToken);
        var employees = await query
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (employees, totalCount);
    }

    public async Task<Employee> AddAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        _context.Employees.Remove(entity);
        return Task.CompletedTask;
    }
}
