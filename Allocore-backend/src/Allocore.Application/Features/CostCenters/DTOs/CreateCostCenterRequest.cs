namespace Allocore.Application.Features.CostCenters.DTOs;

public record CreateCostCenterRequest(
    string Code,
    string Name,
    string? Description
);
