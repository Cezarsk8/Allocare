namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;

public record UpdateProviderCommand(Guid CompanyId, Guid ProviderId, UpdateProviderRequest Request) : IRequest<Result<ProviderDto>>;
