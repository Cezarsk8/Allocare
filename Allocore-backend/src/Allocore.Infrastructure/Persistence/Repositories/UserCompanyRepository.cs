namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Companies;

public class UserCompanyRepository : IUserCompanyRepository
{
    private readonly ApplicationDbContext _context;

    public UserCompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserCompany?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.UserCompanies
            .Include(uc => uc.User)
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.Id == id, cancellationToken);

    public async Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
        => await _context.UserCompanies
            .Include(uc => uc.User)
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);

    public async Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.UserCompanies
            .Include(uc => uc.Company)
            .Where(uc => uc.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
        => await _context.UserCompanies
            .Include(uc => uc.User)
            .Where(uc => uc.CompanyId == companyId)
            .ToListAsync(cancellationToken);

    public async Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
        => await _context.UserCompanies.AnyAsync(
            uc => uc.UserId == userId && uc.CompanyId == companyId, cancellationToken);

    public async Task<bool> UserIsOwnerOfCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default)
        => await _context.UserCompanies.AnyAsync(
            uc => uc.UserId == userId && uc.CompanyId == companyId && uc.RoleInCompany == RoleInCompany.Owner, cancellationToken);

    public async Task<IEnumerable<UserCompany>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.UserCompanies.ToListAsync(cancellationToken);

    public async Task<UserCompany> AddAsync(UserCompany entity, CancellationToken cancellationToken = default)
    {
        await _context.UserCompanies.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(UserCompany entity, CancellationToken cancellationToken = default)
    {
        _context.UserCompanies.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserCompany entity, CancellationToken cancellationToken = default)
    {
        _context.UserCompanies.Remove(entity);
        return Task.CompletedTask;
    }
}
