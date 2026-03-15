namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.CostCenters;

public interface ICostCenterRepository : IReadRepository<CostCenter>, IWriteRepository<CostCenter>
{
    Task<bool> ExistsByCodeInCompanyAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeInCompanyExcludingAsync(Guid companyId, string code, Guid excludeId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<CostCenter> CostCenters, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<int> GetEmployeeCountAsync(Guid costCenterId, CancellationToken cancellationToken = default);
}
