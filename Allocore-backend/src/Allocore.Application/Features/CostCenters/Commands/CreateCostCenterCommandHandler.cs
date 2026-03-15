namespace Allocore.Application.Features.CostCenters.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.CostCenters;

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, Result<CostCenterDto>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(
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

    public async Task<Result<CostCenterDto>> Handle(CreateCostCenterCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<CostCenterDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<CostCenterDto>("You don't have access to this company.");

        if (await _costCenterRepository.ExistsByCodeInCompanyAsync(command.CompanyId, command.Request.Code.Trim().ToUpperInvariant(), cancellationToken))
            return Result.Failure<CostCenterDto>("A cost center with this code already exists in this company.");

        var costCenter = CostCenter.Create(
            command.CompanyId,
            command.Request.Code,
            command.Request.Name,
            command.Request.Description);

        await _costCenterRepository.AddAsync(costCenter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CostCenterDto(
            costCenter.Id,
            costCenter.CompanyId,
            costCenter.Code,
            costCenter.Name,
            costCenter.Description,
            costCenter.IsActive,
            0,
            costCenter.CreatedAt,
            costCenter.UpdatedAt));
    }
}
