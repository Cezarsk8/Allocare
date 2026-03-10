namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Domain.Common;

public record UpdateContractStatusCommand(Guid CompanyId, Guid ContractId, string NewStatus) : IRequest<Result>;
