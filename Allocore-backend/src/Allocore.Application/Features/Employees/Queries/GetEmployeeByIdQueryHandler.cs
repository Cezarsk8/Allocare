namespace Allocore.Application.Features.Employees.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Employees;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetEmployeeByIdQueryHandler(
        IEmployeeRepository employeeRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _employeeRepository = employeeRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<EmployeeDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, request.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<EmployeeDto>("You don't have access to this company.");

        var employee = await _employeeRepository.GetByIdWithCostCenterAsync(request.EmployeeId, cancellationToken);
        if (employee is null || employee.CompanyId != request.CompanyId)
            return Result.Failure<EmployeeDto>("Employee not found.");

        return Result.Success(MapToDto(employee));
    }

    private static EmployeeDto MapToDto(Employee e) => new(
        e.Id, e.CompanyId, e.Name, e.Email,
        e.CostCenterId, e.CostCenter?.Name, e.CostCenter?.Code,
        e.JobTitle, e.HireDate, e.TerminationDate, e.IsActive,
        e.CreatedAt, e.UpdatedAt);
}
