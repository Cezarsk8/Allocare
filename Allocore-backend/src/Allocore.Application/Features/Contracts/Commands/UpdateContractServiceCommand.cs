namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;

public record UpdateContractServiceCommand(Guid CompanyId, Guid ContractId, Guid ServiceId, CreateContractServiceRequest Request) : IRequest<Result<ContractServiceDto>>;
