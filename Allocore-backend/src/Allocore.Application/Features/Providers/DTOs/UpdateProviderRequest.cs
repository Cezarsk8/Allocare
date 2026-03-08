namespace Allocore.Application.Features.Providers.DTOs;

public record UpdateProviderRequest(
    string Name,
    string Category,
    string? LegalName,
    string? TaxId,
    string? Website,
    string? Description
);
