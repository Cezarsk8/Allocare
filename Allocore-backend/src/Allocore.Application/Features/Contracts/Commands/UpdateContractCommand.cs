namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;

public record UpdateContractCommand(Guid CompanyId, Guid ContractId, UpdateContractRequest Request) : IRequest<Result<ContractDto>>;
