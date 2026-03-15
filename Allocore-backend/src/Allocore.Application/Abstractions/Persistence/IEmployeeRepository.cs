namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Employees;

public interface IEmployeeRepository : IReadRepository<Employee>, IWriteRepository<Employee>
{
    Task<Employee?> GetByIdWithCostCenterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailInCompanyAsync(Guid companyId, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailInCompanyExcludingAsync(Guid companyId, string email, Guid excludeId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedByCompanyAsync(
        Guid companyId, int page, int pageSize,
        Guid? costCenterIdFilter = null,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPagedByCostCenterAsync(
        Guid costCenterId, int page, int pageSize,
        CancellationToken cancellationToken = default);
}
