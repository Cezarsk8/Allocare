namespace Allocore.Application.Features.Providers.DTOs;

public record CreateProviderContactRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Role,
    bool IsPrimary
);
