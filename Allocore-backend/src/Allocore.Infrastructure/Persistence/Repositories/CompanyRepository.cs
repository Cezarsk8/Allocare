namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Companies;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
        => await _context.Companies.FirstOrDefaultAsync(c => c.TaxId == taxId, cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.Companies.AnyAsync(c => c.Name == name, cancellationToken);

    public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Companies.ToListAsync(cancellationToken);

    public async Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Companies.CountAsync(cancellationToken);
        var companies = await _context.Companies
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (companies, totalCount);
    }

    public async Task<Company> AddAsync(Company entity, CancellationToken cancellationToken = default)
    {
        await _context.Companies.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Company entity, CancellationToken cancellationToken = default)
    {
        _context.Companies.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Company entity, CancellationToken cancellationToken = default)
    {
        _context.Companies.Remove(entity);
        return Task.CompletedTask;
    }
}
