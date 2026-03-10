namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Contracts.DTOs;

public class GetContractsByProviderQueryHandler : IRequestHandler<GetContractsByProviderQuery, IEnumerable<ContractListItemDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetContractsByProviderQueryHandler(
        IContractRepository contractRepository,
        IProviderRepository providerRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _contractRepository = contractRepository;
        _providerRepository = providerRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ContractListItemDto>> Handle(GetContractsByProviderQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return [];

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, query.CompanyId, cancellationToken);
        if (!hasAccess)
            return [];

        // Verify provider belongs to company
        var provider = await _providerRepository.GetByIdAsync(query.ProviderId, cancellationToken);
        if (provider == null || provider.CompanyId != query.CompanyId)
            return [];

        var contracts = await _contractRepository.GetByProviderAsync(query.ProviderId, cancellationToken);
        return contracts.Select(ContractMapper.ToListItemDto);
    }
}
