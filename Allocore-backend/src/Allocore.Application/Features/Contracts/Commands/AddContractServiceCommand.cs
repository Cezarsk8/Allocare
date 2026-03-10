namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;

public record AddContractServiceCommand(Guid CompanyId, Guid ContractId, CreateContractServiceRequest Request) : IRequest<Result<ContractServiceDto>>;
