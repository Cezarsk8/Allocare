namespace Allocore.Application.Features.Companies.DTOs;

public record UpdateCompanyRequest(
    string Name,
    string? LegalName,
    string? TaxId
);
