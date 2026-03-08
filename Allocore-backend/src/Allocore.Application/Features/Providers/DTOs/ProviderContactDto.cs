namespace Allocore.Application.Features.Providers.DTOs;

public record ProviderContactDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Role,
    bool IsPrimary
);
