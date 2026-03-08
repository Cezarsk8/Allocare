namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Domain.Common;

public record RemoveProviderContactCommand(Guid CompanyId, Guid ProviderId, Guid ContactId) : IRequest<Result>;
