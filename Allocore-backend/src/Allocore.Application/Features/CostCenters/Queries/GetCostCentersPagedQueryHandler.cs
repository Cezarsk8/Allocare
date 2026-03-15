namespace Allocore.Application.Features.CostCenters.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Common;
using Allocore.Application.Features.CostCenters.DTOs;

public class GetCostCentersPagedQueryHandler : IRequestHandler<GetCostCentersPagedQuery, PagedResult<CostCenterListItemDto>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetCostCentersPagedQueryHandler(
        ICostCenterRepository costCenterRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _costCenterRepository = costCenterRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CostCenterListItemDto>> Handle(GetCostCentersPagedQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return new PagedResult<CostCenterListItemDto>(Enumerable.Empty<CostCenterListItemDto>(), request.Page, request.PageSize, 0, 0);

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, request.CompanyId, cancellationToken);
        if (!hasAccess)
            return new PagedResult<CostCenterListItemDto>(Enumerable.Empty<CostCenterListItemDto>(), request.Page, request.PageSize, 0, 0);

        var (costCenters, totalCount) = await _costCenterRepository.GetPagedByCompanyAsync(
            request.CompanyId, request.Page, request.PageSize,
            request.IsActive, request.SearchTerm,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = new List<CostCenterListItemDto>();
        foreach (var cc in costCenters)
        {
            var employeeCount = await _costCenterRepository.GetEmployeeCountAsync(cc.Id, cancellationToken);
            items.Add(new CostCenterListItemDto(cc.Id, cc.Code, cc.Name, cc.IsActive, employeeCount));
        }

        return new PagedResult<CostCenterListItemDto>(items, request.Page, request.PageSize, totalCount, totalPages);
    }
}
