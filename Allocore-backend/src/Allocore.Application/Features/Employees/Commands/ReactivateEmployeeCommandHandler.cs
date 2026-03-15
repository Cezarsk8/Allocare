namespace Allocore.Application.Features.Employees.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;

public class ReactivateEmployeeCommandHandler : IRequestHandler<ReactivateEmployeeCommand, Result>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReactivateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure("You don't have access to this company.");

        var employee = await _employeeRepository.GetByIdAsync(command.EmployeeId, cancellationToken);
        if (employee is null || employee.CompanyId != command.CompanyId)
            return Result.Failure("Employee not found.");

        employee.Reactivate();
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
