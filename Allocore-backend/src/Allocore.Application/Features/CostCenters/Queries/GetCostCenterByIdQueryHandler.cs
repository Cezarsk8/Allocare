namespace Allocore.Application.Features.CostCenters.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Domain.Common;

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, Result<CostCenterDto>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetCostCenterByIdQueryHandler(
        ICostCenterRepository costCenterRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _costCenterRepository = costCenterRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CostCenterDto>> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<CostCenterDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, request.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<CostCenterDto>("You don't have access to this company.");

        var costCenter = await _costCenterRepository.GetByIdAsync(request.CostCenterId, cancellationToken);
        if (costCenter is null || costCenter.CompanyId != request.CompanyId)
            return Result.Failure<CostCenterDto>("Cost center not found.");

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
