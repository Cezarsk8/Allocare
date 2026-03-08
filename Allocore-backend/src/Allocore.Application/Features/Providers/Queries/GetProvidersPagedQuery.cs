namespace Allocore.Application.Features.Providers.Queries;

using MediatR;
using Allocore.Application.Common;
using Allocore.Application.Features.Providers.DTOs;

public record GetProvidersPagedQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    bool? IsActive = null,
    string? SearchTerm = null
) : IRequest<PagedResult<ProviderListItemDto>>;
