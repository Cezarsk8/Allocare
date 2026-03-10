namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Common;
using Allocore.Application.Features.Contracts.DTOs;

public record GetContractsPagedQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 10,
    Guid? ProviderId = null,
    string? Status = null,
    bool? ExpiringOnly = null,
    int ExpiringDays = 30,
    string? SearchTerm = null
) : IRequest<PagedResult<ContractListItemDto>>;
