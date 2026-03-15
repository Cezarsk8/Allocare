namespace Allocore.Application.Features.CostCenters.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;

public class DeactivateCostCenterCommandHandler : IRequestHandler<DeactivateCostCenterCommand, Result>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCostCenterCommandHandler(
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

    public async Task<Result> Handle(DeactivateCostCenterCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure("You don't have access to this company.");

        var costCenter = await _costCenterRepository.GetByIdAsync(command.CostCenterId, cancellationToken);
        if (costCenter is null || costCenter.CompanyId != command.CompanyId)
            return Result.Failure("Cost center not found.");

        costCenter.Deactivate();
        await _costCenterRepository.UpdateAsync(costCenter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
