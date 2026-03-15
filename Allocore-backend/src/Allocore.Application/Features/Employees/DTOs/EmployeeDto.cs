namespace Allocore.Application.Features.Employees.DTOs;

public record EmployeeDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Email,
    Guid? CostCenterId,
    string? CostCenterName,
    string? CostCenterCode,
    string? JobTitle,
    DateTime? HireDate,
    DateTime? TerminationDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
