namespace Allocore.Application.Features.Providers.DTOs;

public record ProviderDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    string? LegalName,
    string? TaxId,
    string Category,
    string? Website,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<ProviderContactDto> Contacts
);
