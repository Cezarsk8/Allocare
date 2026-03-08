namespace Allocore.Application.Features.Providers.DTOs;

public record CreateProviderRequest(
    string Name,
    string Category,
    string? LegalName,
    string? TaxId,
    string? Website,
    string? Description,
    IEnumerable<CreateProviderContactRequest>? Contacts
);
