namespace Allocore.Domain.Entities.Employees;

using Allocore.Domain.Common;
using Allocore.Domain.Entities.CostCenters;

public class Employee : Entity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Guid? CostCenterId { get; private set; }
    public string? JobTitle { get; private set; }
    public DateTime? HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public CostCenter? CostCenter { get; private set; }

    private Employee() { } // EF Core

    public static Employee Create(
        Guid companyId,
        string name,
        string email,
        Guid? costCenterId = null,
        string? jobTitle = null,
        DateTime? hireDate = null)
    {
        return new Employee
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            CostCenterId = costCenterId,
            JobTitle = jobTitle?.Trim(),
            HireDate = NormalizeToUtc(hireDate),
            IsActive = true
        };
    }

    public void Update(
        string name,
        string email,
        Guid? costCenterId,
        string? jobTitle,
        DateTime? hireDate)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        CostCenterId = costCenterId;
        JobTitle = jobTitle?.Trim();
        HireDate = NormalizeToUtc(hireDate);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Terminate(DateTime terminationDate)
    {
        TerminationDate = NormalizeToUtc(terminationDate)!.Value;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (value is null) return null;
        return value.Value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }

    public void Reactivate()
    {
        TerminationDate = null;
        IsActive = true;
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
