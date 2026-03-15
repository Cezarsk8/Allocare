namespace Allocore.Application.Features.Employees.Queries;

using MediatR;
using Allocore.Application.Common;
using Allocore.Application.Features.Employees.DTOs;

public record GetEmployeesByCostCenterQuery(
    Guid CompanyId,
    Guid CostCenterId,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<EmployeeListItemDto>>;
