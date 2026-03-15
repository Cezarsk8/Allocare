namespace Allocore.Application.Features.CostCenters.DTOs;

public record CostCenterDto(
    Guid Id,
    Guid CompanyId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    int EmployeeCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
