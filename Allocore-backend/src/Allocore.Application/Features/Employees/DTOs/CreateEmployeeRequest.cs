namespace Allocore.Application.Features.Employees.DTOs;

public record CreateEmployeeRequest(
    string Name,
    string Email,
    Guid? CostCenterId,
    string? JobTitle,
    DateTime? HireDate
);
