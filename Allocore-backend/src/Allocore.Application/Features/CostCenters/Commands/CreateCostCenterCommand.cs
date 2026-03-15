namespace Allocore.Application.Features.CostCenters.Commands;

using MediatR;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Domain.Common;

public record CreateCostCenterCommand(Guid CompanyId, CreateCostCenterRequest Request) : IRequest<Result<CostCenterDto>>;
