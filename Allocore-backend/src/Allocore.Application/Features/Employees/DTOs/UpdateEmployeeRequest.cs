namespace Allocore.Application.Features.Employees.DTOs;

public record UpdateEmployeeRequest(
    string Name,
    string Email,
    Guid? CostCenterId,
    string? JobTitle,
    DateTime? HireDate
);
