namespace Allocore.Application.Features.Contracts.DTOs;

public record CreateContractServiceRequest(
    string ServiceName,
    string? ServiceDescription,
    decimal? UnitPrice,
    string? UnitType,
    int? Quantity,
    string? Notes
);
