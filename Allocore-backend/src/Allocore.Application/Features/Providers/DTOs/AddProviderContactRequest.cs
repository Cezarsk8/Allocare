namespace Allocore.Application.Features.Providers.DTOs;

public record AddProviderContactRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Role,
    bool IsPrimary
);
