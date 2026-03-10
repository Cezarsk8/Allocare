namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;

public record GetExpiringContractsQuery(Guid CompanyId, int WithinDays = 30) : IRequest<IEnumerable<ContractListItemDto>>;
