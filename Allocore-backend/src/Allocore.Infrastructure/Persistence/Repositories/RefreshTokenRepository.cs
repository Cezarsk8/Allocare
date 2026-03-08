namespace Allocore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Entities.Users;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;
    
    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
    
    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    
    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);
        
        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }
    
    public async Task<IEnumerable<RefreshToken>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.ToListAsync(cancellationToken);
    
    public async Task<RefreshToken> AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(entity, cancellationToken);
        return entity;
    }
    
    public Task UpdateAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(entity);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(RefreshToken entity, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Remove(entity);
        return Task.CompletedTask;
    }
}
