namespace Allocore.Application.Features.CostCenters.Commands;

using MediatR;
using Allocore.Domain.Common;

public record ActivateCostCenterCommand(Guid CompanyId, Guid CostCenterId) : IRequest<Result>;
