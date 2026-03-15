namespace Allocore.Application.Features.CostCenters.Queries;

using MediatR;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Domain.Common;

public record GetCostCenterByIdQuery(Guid CompanyId, Guid CostCenterId) : IRequest<Result<CostCenterDto>>;
