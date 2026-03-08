namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;

public record UpdateProviderContactCommand(Guid CompanyId, Guid ProviderId, Guid ContactId, UpdateProviderContactRequest Request) : IRequest<Result<ProviderContactDto>>;
