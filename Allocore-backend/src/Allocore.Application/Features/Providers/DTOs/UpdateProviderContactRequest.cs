namespace Allocore.Application.Features.Providers.DTOs;

public record UpdateProviderContactRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Role,
    bool IsPrimary
);
