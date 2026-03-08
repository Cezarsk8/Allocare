namespace Allocore.Domain.Entities.Providers;

using Allocore.Domain.Common;

public class ProviderContact : Entity
{
    public Guid ProviderId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Role { get; private set; }
    public bool IsPrimary { get; private set; }

    // Navigation property
    public Provider? Provider { get; private set; }

    private ProviderContact() { } // EF Core

    public static ProviderContact Create(
        Guid providerId,
        string name,
        string? email = null,
        string? phone = null,
        string? role = null,
        bool isPrimary = false)
    {
        return new ProviderContact
        {
            ProviderId = providerId,
            Name = name,
            Email = email,
            Phone = phone,
            Role = role,
            IsPrimary = isPrimary
        };
    }

    public void Update(string name, string? email, string? phone, string? role, bool isPrimary)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Role = role;
        IsPrimary = isPrimary;
        UpdatedAt = DateTime.UtcNow;
    }
}
