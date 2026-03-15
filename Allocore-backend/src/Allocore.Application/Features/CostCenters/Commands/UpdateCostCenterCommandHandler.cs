namespace Allocore.Application.Features.CostCenters.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Domain.Common;

public class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, Result<CostCenterDto>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCostCenterCommandHandler(
        ICostCenterRepository costCenterRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CostCenterDto>> Handle(UpdateCostCenterCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<CostCenterDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<CostCenterDto>("You don't have access to this company.");

        var costCenter = await _costCenterRepository.GetByIdAsync(command.CostCenterId, cancellationToken);
        if (costCenter is null || costCenter.CompanyId != command.CompanyId)
            return Result.Failure<CostCenterDto>("Cost center not found.");

        if (await _costCenterRepository.ExistsByCodeInCompanyExcludingAsync(
                command.CompanyId, command.Request.Code.Trim().ToUpperInvariant(), command.CostCenterId, cancellationToken))
            return Result.Failure<CostCenterDto>("A cost center with this code already exists in this company.");

        costCenter.Update(command.Request.Code, command.Request.Name, command.Request.Description);

        await _costCenterRepository.UpdateAsync(costCenter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var employeeCount = await _costCenterRepository.GetEmployeeCountAsync(costCenter.Id, cancellationToken);

        return Result.Success(new CostCenterDto(
            costCenter.Id,
            costCenter.CompanyId,
            costCenter.Code,
            costCenter.Name,
            costCenter.Description,
            costCenter.IsActive,
            employeeCount,
            costCenter.CreatedAt,
            costCenter.UpdatedAt));
    }
}
