namespace Allocore.Domain.Entities.Contracts;

public enum ContractStatus
{
    Draft = 0,
    InNegotiation = 1,
    Active = 2,
    Expiring = 3,
    Expired = 4,
    Renewed = 5,
    Cancelled = 6,
    Terminated = 7
}
