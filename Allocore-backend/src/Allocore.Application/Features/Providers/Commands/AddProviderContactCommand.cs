namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;

public record AddProviderContactCommand(Guid CompanyId, Guid ProviderId, AddProviderContactRequest Request) : IRequest<Result<ProviderContactDto>>;
