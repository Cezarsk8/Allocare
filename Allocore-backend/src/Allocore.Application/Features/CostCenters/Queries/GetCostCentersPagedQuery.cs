namespace Allocore.Application.Features.CostCenters.Queries;

using MediatR;
using Allocore.Application.Common;
using Allocore.Application.Features.CostCenters.DTOs;

public record GetCostCentersPagedQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 10,
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<PagedResult<CostCenterListItemDto>>;
