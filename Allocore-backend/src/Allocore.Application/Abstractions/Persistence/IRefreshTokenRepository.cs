namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Users;

public interface IRefreshTokenRepository : IReadRepository<RefreshToken>, IWriteRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
