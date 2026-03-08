namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Providers;

public interface IProviderRepository : IReadRepository<Provider>, IWriteRepository<Provider>
{
    Task<Provider?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameInCompanyAsync(Guid companyId, string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameInCompanyExcludingAsync(Guid companyId, string name, Guid excludeProviderId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Provider> Providers, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        ProviderCategory? categoryFilter = null,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Provider>> GetAllByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
}
