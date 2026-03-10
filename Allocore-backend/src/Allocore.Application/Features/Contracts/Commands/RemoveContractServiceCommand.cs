namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Domain.Common;

public record RemoveContractServiceCommand(Guid CompanyId, Guid ContractId, Guid ServiceId) : IRequest<Result>;
