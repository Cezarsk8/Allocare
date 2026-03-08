namespace Allocore.Application.Features.Companies.DTOs;

public record AddUserToCompanyRequest(
    Guid UserId,
    string RoleInCompany
);
