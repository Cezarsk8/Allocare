namespace Allocore.Application.Features.Providers.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Common;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Entities.Providers;

public class GetProvidersPagedQueryHandler : IRequestHandler<GetProvidersPagedQuery, PagedResult<ProviderListItemDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetProvidersPagedQueryHandler(
        IProviderRepository providerRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _providerRepository = providerRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProviderListItemDto>> Handle(GetProvidersPagedQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return new PagedResult<ProviderListItemDto>(Enumerable.Empty<ProviderListItemDto>(), request.Page, request.PageSize, 0, 0);

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, request.CompanyId, cancellationToken);
        if (!hasAccess)
            return new PagedResult<ProviderListItemDto>(Enumerable.Empty<ProviderListItemDto>(), request.Page, request.PageSize, 0, 0);

        // Parse category filter
        ProviderCategory? categoryFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Category) && Enum.TryParse<ProviderCategory>(request.Category, true, out var cat))
            categoryFilter = cat;

        var (providers, totalCount) = await _providerRepository.GetPagedByCompanyAsync(
            request.CompanyId, request.Page, request.PageSize,
            categoryFilter, request.IsActive, request.SearchTerm,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = providers.Select(p => new ProviderListItemDto(
            p.Id,
            p.Name,
            p.Category.ToString(),
            p.Website,
            p.IsActive,
            p.Contacts.Count,
            p.Contacts.FirstOrDefault(c => c.IsPrimary)?.Name
        ));

        return new PagedResult<ProviderListItemDto>(items, request.Page, request.PageSize, totalCount, totalPages);
    }
}
