namespace Allocore.Domain.Entities.CostCenters;

using Allocore.Domain.Common;

public class CostCenter : Entity
{
    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private CostCenter() { } // EF Core

    public static CostCenter Create(
        Guid companyId,
        string code,
        string name,
        string? description = null)
    {
        return new CostCenter
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    public void Update(string code, string name, string? description)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description?.Trim();
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
