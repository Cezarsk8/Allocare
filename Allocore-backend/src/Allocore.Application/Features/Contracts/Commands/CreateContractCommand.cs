namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;

public record CreateContractCommand(Guid CompanyId, CreateContractRequest Request) : IRequest<Result<ContractDto>>;
