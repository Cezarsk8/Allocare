namespace Allocore.Application.Features.Companies.DTOs;

public record CompanyDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxId,
    bool IsActive,
    DateTime CreatedAt,
    string? UserRole
);
