namespace Allocore.Application.Features.Contracts.DTOs;

public record ContractServiceDto(
    Guid Id,
    string ServiceName,
    string? ServiceDescription,
    decimal? UnitPrice,
    string? UnitType,
    int? Quantity,
    string? Notes
);
