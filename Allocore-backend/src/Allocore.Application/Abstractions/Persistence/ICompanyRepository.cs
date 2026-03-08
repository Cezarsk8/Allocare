namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Companies;

public interface ICompanyRepository : IReadRepository<Company>, IWriteRepository<Company>
{
    Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
