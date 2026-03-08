namespace Allocore.Domain.Entities.Providers;

using Allocore.Domain.Common;

public class Provider : Entity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxId { get; private set; }
    public ProviderCategory Category { get; private set; }
    public string? Website { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ProviderContact> _contacts = new();
    public IReadOnlyCollection<ProviderContact> Contacts => _contacts.AsReadOnly();

    private Provider() { } // EF Core

    public static Provider Create(
        Guid companyId,
        string name,
        ProviderCategory category,
        string? legalName = null,
        string? taxId = null,
        string? website = null,
        string? description = null)
    {
        return new Provider
        {
            CompanyId = companyId,
            Name = name,
            Category = category,
            LegalName = legalName,
            TaxId = taxId,
            Website = website,
            Description = description,
            IsActive = true
        };
    }

    public void Update(
        string name,
        ProviderCategory category,
        string? legalName,
        string? taxId,
        string? website,
        string? description)
    {
        Name = name;
        Category = category;
        LegalName = legalName;
        TaxId = taxId;
        Website = website;
        Description = description;
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

    public void AddContact(ProviderContact contact)
    {
        _contacts.Add(contact);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveContact(ProviderContact contact)
    {
        _contacts.Remove(contact);
        UpdatedAt = DateTime.UtcNow;
    }
}
