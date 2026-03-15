namespace Allocore.Application.Features.Employees.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Common;
using Allocore.Application.Features.Employees.DTOs;

public class GetEmployeesPagedQueryHandler : IRequestHandler<GetEmployeesPagedQuery, PagedResult<EmployeeListItemDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetEmployeesPagedQueryHandler(
        IEmployeeRepository employeeRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _employeeRepository = employeeRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<EmployeeListItemDto>> Handle(GetEmployeesPagedQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return new PagedResult<EmployeeListItemDto>(Enumerable.Empty<EmployeeListItemDto>(), request.Page, request.PageSize, 0, 0);

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, request.CompanyId, cancellationToken);
        if (!hasAccess)
            return new PagedResult<EmployeeListItemDto>(Enumerable.Empty<EmployeeListItemDto>(), request.Page, request.PageSize, 0, 0);

        var (employees, totalCount) = await _employeeRepository.GetPagedByCompanyAsync(
            request.CompanyId, request.Page, request.PageSize,
            request.CostCenterId, request.IsActive, request.SearchTerm,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = employees.Select(e => new EmployeeListItemDto(
            e.Id, e.Name, e.Email,
            e.CostCenter?.Name, e.JobTitle, e.IsActive));

        return new PagedResult<EmployeeListItemDto>(items, request.Page, request.PageSize, totalCount, totalPages);
    }
}
