namespace Allocore.Application.Features.CostCenters.DTOs;

public record CostCenterListItemDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    int EmployeeCount
);
