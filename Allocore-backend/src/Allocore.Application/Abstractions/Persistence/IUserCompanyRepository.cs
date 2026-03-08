namespace Allocore.Application.Abstractions.Persistence;

using Allocore.Domain.Entities.Companies;

public interface IUserCompanyRepository : IReadRepository<UserCompany>, IWriteRepository<UserCompany>
{
    Task<UserCompany?> GetByUserAndCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserCompany>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserCompany>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> UserHasAccessToCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> UserIsOwnerOfCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken = default);
}
