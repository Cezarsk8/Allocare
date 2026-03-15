namespace Allocore.Application.Features.Employees.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Employees;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        ICostCenterRepository costCenterRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _costCenterRepository = costCenterRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<EmployeeDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<EmployeeDto>("You don't have access to this company.");

        var employee = await _employeeRepository.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null || employee.CompanyId != command.CompanyId)
            return Result.Failure<EmployeeDto>("Employee not found.");

        if (await _employeeRepository.ExistsByEmailInCompanyExcludingAsync(
                command.CompanyId, command.Request.Email.Trim().ToLowerInvariant(), command.EmployeeId, cancellationToken))
            return Result.Failure<EmployeeDto>("An employee with this email already exists in this company.");

        // Validate CostCenterId belongs to same company
        if (command.Request.CostCenterId.HasValue)
        {
            var costCenter = await _costCenterRepository.GetByIdAsync(command.Request.CostCenterId.Value, cancellationToken);
            if (costCenter is null || costCenter.CompanyId != command.CompanyId)
                return Result.Failure<EmployeeDto>("Cost center not found in this company.");
        }

        employee.Update(
            command.Request.Name,
            command.Request.Email,
            command.Request.CostCenterId,
            command.Request.JobTitle,
            command.Request.HireDate);

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _employeeRepository.GetByIdWithCostCenterAsync(employee.Id, cancellationToken);

        return Result.Success(MapToDto(loaded!));
    }

    private static EmployeeDto MapToDto(Employee e) => new(
        e.Id, e.CompanyId, e.Name, e.Email,
        e.CostCenterId, e.CostCenter?.Name, e.CostCenter?.Code,
        e.JobTitle, e.HireDate, e.TerminationDate, e.IsActive,
        e.CreatedAt, e.UpdatedAt);
}
