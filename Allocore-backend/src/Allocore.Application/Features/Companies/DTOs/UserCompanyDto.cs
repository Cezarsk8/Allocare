namespace Allocore.Application.Features.Companies.DTOs;

public record UserCompanyDto(
    Guid UserId,
    string UserEmail,
    string UserFullName,
    Guid CompanyId,
    string CompanyName,
    string RoleInCompany,
    DateTime JoinedAt
);
