namespace Allocore.Application.Features.Contracts.DTOs;

public record ContractListItemDto(
    Guid Id,
    string Title,
    string? ContractNumber,
    string ProviderName,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    string BillingFrequency,
    decimal? TotalValue,
    string? Currency,
    bool IsExpired,
    bool IsExpiringSoon,
    int ServiceCount
);
