namespace Allocore.Application.Features.Providers.DTOs;

public record ProviderListItemDto(
    Guid Id,
    string Name,
    string Category,
    string? Website,
    bool IsActive,
    int ContactCount,
    string? PrimaryContactName
);
