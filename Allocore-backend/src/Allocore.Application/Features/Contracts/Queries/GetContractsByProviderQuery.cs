namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;

public record GetContractsByProviderQuery(Guid CompanyId, Guid ProviderId) : IRequest<IEnumerable<ContractListItemDto>>;
