namespace Allocore.Domain.Entities.Companies;

using Allocore.Domain.Common;
using Allocore.Domain.Entities.Users;

public class UserCompany : Entity
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public RoleInCompany RoleInCompany { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Company? Company { get; private set; }

    private UserCompany() { } // EF Core

    public static UserCompany Create(Guid userId, Guid companyId, RoleInCompany roleInCompany)
    {
        return new UserCompany
        {
            UserId = userId,
            CompanyId = companyId,
            RoleInCompany = roleInCompany
        };
    }

    public void UpdateRole(RoleInCompany newRole)
    {
        RoleInCompany = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
}
