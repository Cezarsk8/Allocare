namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Contracts;

public interface IContractRepository : IReadRepository<Contract>, IWriteRepository<Contract>
{
    Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByContractNumberInCompanyAsync(Guid companyId, string contractNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByContractNumberInCompanyExcludingAsync(Guid companyId, string contractNumber, Guid excludeContractId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Contract> Contracts, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        Guid? providerIdFilter = null,
        ContractStatus? statusFilter = null,
        bool? expiringWithinDays = null,
        int expiringDays = 30,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Contract>> GetByProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contract>> GetExpiringContractsAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contract>> GetContractsNeedingRenewalAsync(Guid companyId, int withinDays, CancellationToken cancellationToken = default);
}
