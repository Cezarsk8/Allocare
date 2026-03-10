namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Common;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Entities.Contracts;

public class GetContractsPagedQueryHandler : IRequestHandler<GetContractsPagedQuery, PagedResult<ContractListItemDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetContractsPagedQueryHandler(
        IContractRepository contractRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _contractRepository = contractRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ContractListItemDto>> Handle(GetContractsPagedQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return new PagedResult<ContractListItemDto>([], 1, query.PageSize, 0, 0);

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, query.CompanyId, cancellationToken);
        if (!hasAccess)
            return new PagedResult<ContractListItemDto>([], 1, query.PageSize, 0, 0);

        // Parse status filter
        ContractStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<ContractStatus>(query.Status, true, out var parsed))
            statusFilter = parsed;

        var (contracts, totalCount) = await _contractRepository.GetPagedByCompanyAsync(
            query.CompanyId, query.Page, query.PageSize,
            query.ProviderId, statusFilter, query.ExpiringOnly, query.ExpiringDays,
            query.SearchTerm, cancellationToken);

        var items = contracts.Select(ContractMapper.ToListItemDto);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResult<ContractListItemDto>(items, query.Page, query.PageSize, totalCount, totalPages);
    }
}
