namespace Allocore.Application.Features.Companies.DTOs;

public record CreateCompanyRequest(
    string Name,
    string? LegalName,
    string? TaxId
);
