namespace Allocore.Application.Features.Employees.DTOs;

public record EmployeeListItemDto(
    Guid Id,
    string Name,
    string Email,
    string? CostCenterName,
    string? JobTitle,
    bool IsActive
);
