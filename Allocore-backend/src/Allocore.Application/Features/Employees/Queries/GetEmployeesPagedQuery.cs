namespace Allocore.Application.Features.Employees.Queries;

using MediatR;
using Allocore.Application.Common;
using Allocore.Application.Features.Employees.DTOs;

public record GetEmployeesPagedQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 10,
    Guid? CostCenterId = null,
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<PagedResult<EmployeeListItemDto>>;
