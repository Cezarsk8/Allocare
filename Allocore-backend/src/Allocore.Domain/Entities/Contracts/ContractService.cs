namespace Allocore.Domain.Entities.Contracts;

using Allocore.Domain.Common;

public class ContractService : Entity
{
    public Guid ContractId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public string? ServiceDescription { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? UnitType { get; private set; }
    public int? Quantity { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Contract? Contract { get; private set; }

    private ContractService() { } // EF Core

    public static ContractService Create(
        Guid contractId,
        string serviceName,
        string? serviceDescription = null,
        decimal? unitPrice = null,
        string? unitType = null,
        int? quantity = null,
        string? notes = null)
    {
        return new ContractService
        {
            ContractId = contractId,
            ServiceName = serviceName,
            ServiceDescription = serviceDescription,
            UnitPrice = unitPrice,
            UnitType = unitType,
            Quantity = quantity,
            Notes = notes
        };
    }

    public void Update(
        string serviceName,
        string? serviceDescription,
        decimal? unitPrice,
        string? unitType,
        int? quantity,
        string? notes)
    {
        ServiceName = serviceName;
        ServiceDescription = serviceDescription;
        UnitPrice = unitPrice;
        UnitType = unitType;
        Quantity = quantity;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
