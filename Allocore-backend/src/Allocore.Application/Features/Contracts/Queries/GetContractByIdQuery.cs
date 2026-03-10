namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;

public record GetContractByIdQuery(Guid CompanyId, Guid ContractId) : IRequest<Result<ContractDto>>;
