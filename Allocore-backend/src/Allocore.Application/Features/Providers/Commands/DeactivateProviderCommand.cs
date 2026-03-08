namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Domain.Common;

public record DeactivateProviderCommand(Guid CompanyId, Guid ProviderId) : IRequest<Result>;
