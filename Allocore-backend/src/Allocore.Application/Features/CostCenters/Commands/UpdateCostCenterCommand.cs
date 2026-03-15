namespace Allocore.Application.Features.CostCenters.Commands;

using MediatR;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Domain.Common;

public record UpdateCostCenterCommand(Guid CompanyId, Guid CostCenterId, UpdateCostCenterRequest Request) : IRequest<Result<CostCenterDto>>;
