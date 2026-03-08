namespace Allocore.Application.Features.Providers.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;

public class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, Result<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetProviderByIdQueryHandler(
        IProviderRepository providerRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _providerRepository = providerRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ProviderDto>> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ProviderDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, request.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ProviderDto>("You don't have access to this company.");

        var provider = await _providerRepository.GetByIdWithContactsAsync(request.ProviderId, cancellationToken);
        if (provider is null || provider.CompanyId != request.CompanyId)
            return Result.Failure<ProviderDto>("Provider not found.");

        return Result.Success(new ProviderDto(
            provider.Id,
            provider.CompanyId,
            provider.Name,
            provider.LegalName,
            provider.TaxId,
            provider.Category.ToString(),
            provider.Website,
            provider.Description,
            provider.IsActive,
            provider.CreatedAt,
            provider.UpdatedAt,
            provider.Contacts.Select(c => new ProviderContactDto(
                c.Id, c.Name, c.Email, c.Phone, c.Role, c.IsPrimary))
        ));
    }
}
