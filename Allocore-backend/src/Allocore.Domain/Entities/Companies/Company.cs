namespace Allocore.Domain.Entities.Companies;

using Allocore.Domain.Common;

public class Company : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<UserCompany> _userCompanies = new();
    public IReadOnlyCollection<UserCompany> UserCompanies => _userCompanies.AsReadOnly();

    private Company() { } // EF Core

    public static Company Create(string name, string? legalName = null, string? taxId = null)
    {
        return new Company
        {
            Name = name,
            LegalName = legalName,
            TaxId = taxId,
            IsActive = true
        };
    }

    public void Update(string name, string? legalName, string? taxId)
    {
        Name = name;
        LegalName = legalName;
        TaxId = taxId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
