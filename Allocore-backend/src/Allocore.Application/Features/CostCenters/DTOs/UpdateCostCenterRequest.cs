namespace Allocore.Application.Features.CostCenters.DTOs;

public record UpdateCostCenterRequest(
    string Code,
    string Name,
    string? Description
);
