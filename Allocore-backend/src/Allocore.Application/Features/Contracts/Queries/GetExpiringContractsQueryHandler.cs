namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Contracts.DTOs;

public class GetExpiringContractsQueryHandler : IRequestHandler<GetExpiringContractsQuery, IEnumerable<ContractListItemDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetExpiringContractsQueryHandler(
        IContractRepository contractRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _contractRepository = contractRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ContractListItemDto>> Handle(GetExpiringContractsQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return [];

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, query.CompanyId, cancellationToken);
        if (!hasAccess)
            return [];

        var contracts = await _contractRepository.GetExpiringContractsAsync(
            query.CompanyId, query.WithinDays, cancellationToken);

        return contracts.Select(ContractMapper.ToListItemDto);
    }
}
